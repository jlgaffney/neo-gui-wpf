using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Neo.Core;
using Neo.Implementations.Wallets.NEP6;
using Neo.IO.Json;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Core.Data;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Globalization.Resources;
using Neo.UI.Core.Helpers;
using Neo.UI.Core.Helpers.Extensions;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Implementations.Exceptions;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;
using Neo.UI.Core.Transactions.Testing;
using Neo.UI.Core.Wallet.Data;
using Neo.UI.Core.Wallet.Exceptions;
using Neo.UI.Core.Wallet.Helpers;
using Neo.UI.Core.Wallet.Messages;
using Neo.VM;
using Neo.Wallets;
using BaseWallet = Neo.Wallets.Wallet;
using DeprecatedWallet = Neo.Implementations.Wallets.EntityFramework.UserWallet;
using ECPoint = Neo.Cryptography.ECC.ECPoint;
using Timer = System.Timers.Timer;

namespace Neo.UI.Core.Wallet
{
    internal class WalletController : IWalletController
    {
        #region Private Fields 
        private readonly Fixed8 NetworkFee = Fixed8.FromDecimal(0.001m);

        private readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly IBlockchainService blockchainService;
        private readonly ICertificateQueryService certificateQueryService;
        private readonly IMessagePublisher messagePublisher;
        private readonly INEP5QueryService nep5QueryService;
        private readonly INodeController nodeController;
        private readonly INotificationService notificationService;
        private readonly ITransactionBuilderFactory transactionBuilderFactory;
        
        private readonly string blockchainDataDirectoryPath;

        private readonly int localNodePort;
        private readonly int localWSPort;

        private readonly string certificateCachePath;

        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        private readonly object walletRefreshLock = new object();

        private bool initialized;
        private bool disposed;

        private Timer refreshTimer;

        private BaseWallet currentWallet;
        private IDisposable currentWalletLocker;
        private WalletInfo currentWalletInfo;

        private bool balanceChanged;
        private bool checkNep5Balance;

        private UInt160[] nep5WatchScriptHashes;

        #endregion

        #region Constructor 
        public WalletController(
            IBlockchainService blockchainService,
            ICertificateQueryService certificateQueryService,
            IMessagePublisher messagePublisher,
            INEP5QueryService nep5QueryService,
            INodeController nodeController,
            INotificationService notificationService,
            ISettingsManager settingsManager,
            ITransactionBuilderFactory transactionBuilderFactory)
        {
            this.blockchainService = blockchainService;
            this.certificateQueryService = certificateQueryService;
            this.messagePublisher = messagePublisher;
            this.nep5QueryService = nep5QueryService;
            this.nodeController = nodeController;
            this.notificationService = notificationService;
            this.transactionBuilderFactory = transactionBuilderFactory;

            this.blockchainDataDirectoryPath = settingsManager.BlockchainDataDirectoryPath;

            this.localNodePort = settingsManager.LocalNodePort;
            this.localWSPort = settingsManager.LocalWSPort;

            this.certificateCachePath = settingsManager.CertificateCachePath;

            this.certificateQueryResultCache = new Dictionary<ECPoint, CertificateQueryResult>();

            StateReader.Default.Notify += Default_Notify;
            StateReader.Default.Log += Default_Log;
        }

        private void Default_Log(object sender, LogEventArgs e)
        {
        }

        private void Default_Notify(object sender, NotifyEventArgs e)
        {
        }
        #endregion

        #region IWalletController implementation
        
        public void Initialize()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IWalletController));
            }

            if (this.initialized)
            {
                throw new ObjectAlreadyInitializedException(nameof(IWalletController));
            }

            this.nodeController.Initialize(this.localNodePort, this.localWSPort);
            this.blockchainService.Initialize(this.blockchainDataDirectoryPath);
            this.certificateQueryService.Initialize(this.certificateCachePath);

            this.blockchainService.BlockAdded += this.OnBlockAdded;

            // Setup automatic refresh timer
            this.refreshTimer = new Timer
            {
                Interval = 1000,
                Enabled = true,
                AutoReset = true
            };

            this.refreshTimer.Elapsed += (sender, e) => this.Refresh();

            this.initialized = true;
        }

        public bool WalletIsOpen => this.currentWallet != null;

        public uint WalletHeight => !this.WalletIsOpen ? 0 : this.currentWallet.WalletHeight;

        public bool WalletIsSynchronized => this.WalletHeight > this.blockchainService.BlockHeight + 1;

        public bool WalletCanBeMigrated(string walletPath)
        {
            return Path.GetExtension(walletPath) == ".db3";
        }

        public string MigrateWallet(string walletPath, string password)
        {
            var newWalletPath = Path.ChangeExtension(walletPath, ".json");
            newWalletPath = PathHelper.GetAvailableFilePath(newWalletPath);

            NEP6Wallet nep6Wallet;
            try
            {
                nep6Wallet = NEP6Wallet.Migrate(newWalletPath, walletPath, password);
            }
            catch (CryptographicException)
            {
                this.notificationService.ShowErrorNotification(Strings.PasswordIncorrect);
                return null;
            }

            // Migration successful
            nep6Wallet.Save();
            nep6Wallet.Dispose();

            this.notificationService.ShowInformationNotification(Strings.MigrateWalletSucceedMessage + newWalletPath);

            return newWalletPath;
        }

        public void CreateWallet(string walletPath, string password, bool createWithAccount = true)
        {
            var newWallet = new NEP6Wallet(walletPath);

            var walletLocker = newWallet.Unlock(password);

            this.SetCurrentWallet(newWallet, walletLocker);

            if (createWithAccount)
            {
                this.CreateAccount();
            }
        }

        public void OpenWallet(string walletPath, string password)
        {
            BaseWallet wallet;
            IDisposable walletLocker;
            if (Path.GetExtension(walletPath) == ".db3")
            {
                DeprecatedWallet userWallet;
                try
                {
                    userWallet = DeprecatedWallet.Open(walletPath, password);
                    walletLocker = null;
                }
                catch (CryptographicException)
                {
                    this.notificationService.ShowErrorNotification(Strings.PasswordIncorrect);
                    return;
                }
                wallet = userWallet;
            }
            else
            {
                var nep6Wallet = new NEP6Wallet(walletPath);
                try
                {
                    walletLocker = nep6Wallet.Unlock(password);
                }
                catch (CryptographicException)
                {
                    this.notificationService.ShowErrorNotification(Strings.PasswordIncorrect);
                    return;
                }
                wallet = nep6Wallet;
            }

            if (wallet == null)
            {
                // TODO Localise text
                this.notificationService.ShowErrorNotification("Could not open wallet! An error occurred while opening");
                return;
            }
            
            this.SetCurrentWallet(wallet, walletLocker);
        }

        public void CloseWallet()
        {
            this.SetCurrentWallet(null, null);
        }

        public void CreateAccount(Contract contract = null)
        {
            this.ThrowIfWalletIsNotOpen();

            WalletAccount account;
            if (contract == null)
            {
                account = this.currentWallet.CreateAccount();
            }
            else
            {
                account = this.currentWallet.CreateAccount(contract);
            }

            this.AddAccountItem(account);

            this.TrySaveWallet();
        }

        public void ImportPrivateKeys(IEnumerable<string> wifPrivateKeys)
        {
            if (wifPrivateKeys == null) return;

            var wifList = wifPrivateKeys.ToList();

            if (!wifList.Any()) return;

            foreach (var wif in wifList)
            {
                WalletAccount account;
                try
                {
                    account = this.currentWallet.Import(wif);
                }
                catch (FormatException)
                {
                    // Skip WIF
                    continue;
                }

                this.AddAccountItem(account);
            }

            this.TrySaveWallet();
        }

        public void ImportCertificate(X509Certificate2 certificate)
        {
            if (certificate == null) return;

            WalletAccount account;
            try
            {
                account = this.currentWallet.Import(certificate);
            }
            catch
            {
                // TODO Localise this text
                this.notificationService.ShowErrorNotification("Certificate import failed!");
                return;
            }

            this.AddAccountItem(account);

            this.TrySaveWallet();
        }
        
        public TestForGasUsageResult TestTransactionForGasUsage(InvokeContractTransactionParameters parameters)
        {
            var builder = this.transactionBuilderFactory.GetBuilder(parameters);

            var transaction = builder.Build(parameters) as InvocationTransaction;

            if (transaction == null)
            {
                return new TestForGasUsageResult(null, null, true);
            }

            transaction.Version = 1;

            // Load default transaction values if required
            if (transaction.Attributes == null) transaction.Attributes = new TransactionAttribute[0];
            if (transaction.Inputs == null) transaction.Inputs = new CoinReference[0];
            if (transaction.Outputs == null) transaction.Outputs = new TransactionOutput[0];
            if (transaction.Scripts == null) transaction.Scripts = new Witness[0];

            var transactionExecutionFailed = false;
            var transactionFee = Fixed8.Zero;

            var engine = ApplicationEngine.Run(parameters.Script, transaction);

            // Get transaction test results
            //var stringBuilder = new StringBuilder();
            //stringBuilder.AppendLine($"VM State: {engine.State}");
            //stringBuilder.AppendLine($"Gas Consumed: {engine.GasConsumed}");
            //stringBuilder.AppendLine($"Evaluation Stack: {new JArray(engine.EvaluationStack.Select(p => p.ToParameter().ToJson()))}");

            if (!engine.State.HasFlag(VMState.FAULT))
            {
                transaction.Gas = engine.GasConsumed - Fixed8.FromDecimal(10);

                if (transaction.Gas < Fixed8.Zero) transaction.Gas = Fixed8.Zero;

                transaction.Gas = transaction.Gas.Ceiling();

                transactionFee = transaction.Gas.Equals(Fixed8.Zero)
                    ? NetworkFee
                    : transaction.Gas;
            }
            else
            {
                transactionExecutionFailed = true;
            }

            return new TestForGasUsageResult(GetInvocationTestResult(engine), transactionFee.ToString(), transactionExecutionFailed);
        }

        public void BuildSignAndRelayTransaction<TParameters>(TParameters transactionParameters) where TParameters : TransactionParameters
        {
            Guard.ArgumentIsNotNull(transactionParameters, nameof(transactionParameters));

            var transaction = this.BuildTransaction(transactionParameters);

            this.SignAndRelay(transaction);
        }

        public void SignAndRelay(Transaction transaction)
        {
            Guard.ArgumentIsNotNull(transaction, nameof(transaction));

            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(transaction);
            }
            catch (InvalidOperationException)
            {
                this.notificationService.ShowErrorNotification(Strings.UnsynchronizedBlock);
                return;
            }

            this.Sign(context);

            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();

                this.Relay(transaction);

                this.notificationService.ShowSuccessNotification($"{Strings.SendTxSucceedMessage} {transaction.Hash}");
            }
            else
            {
                this.notificationService.ShowSuccessNotification($"{Strings.IncompletedSignatureMessage} {context}");
            }
        }

        public bool Sign(ContractParametersContext context)
        {
            Guard.ArgumentIsNotNull(context, nameof(context));

            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.Sign(context);
        }

        public void Relay(Transaction transaction, bool saveTransaction = true)
        {
            Guard.ArgumentIsNotNull(transaction, nameof(transaction));

            this.nodeController.Relay(transaction);

            if (!saveTransaction) return;

            this.currentWallet.ApplyTransaction(transaction);
        }

        public void Relay(IInventory inventory)
        {
            Guard.ArgumentIsNotNull(inventory, nameof(inventory));

            this.nodeController.Relay(inventory);
        }

        public void SetNEP5WatchScriptHashes(IEnumerable<string> nep5WatchScriptHashesHex)
        {
            var scriptHashes = new List<UInt160>();

            foreach (var scriptHashHex in nep5WatchScriptHashesHex)
            {
                if (!UInt160.TryParse(scriptHashHex, out var scriptHash)) continue;

                scriptHashes.Add(scriptHash);
            }

            this.nep5WatchScriptHashes = scriptHashes.ToArray();
        }

        public IEnumerable<UInt160> GetNEP5WatchScriptHashes()
        {
            return this.nep5WatchScriptHashes;
        }

        public IEnumerable<WalletAccount> GetAccounts()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.GetAccounts();
        }

        public IEnumerable<string> GetAccountsAddresses()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts()
                .Select(x => x.Address)
                .ToList();
        }

        public IEnumerable<WalletAccount> GetNonWatchOnlyAccounts()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts().Where(account => !account.WatchOnly);
        }

        public IEnumerable<WalletAccount> GetStandardAccounts()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts()
                .Where(account => !account.WatchOnly && account.Contract.IsStandard);
        }

        public IEnumerable<AssetDto> GetWalletAssets()
        {
            var walletAssets = new List<AssetDto>();

            // Add First-Class assets
            var unspendAssetId = this.FindUnspentAssetIds();

            foreach(var assetId in unspendAssetId)
            {
                var assetState = this.blockchainService.GetAssetState(assetId);

                walletAssets.Add(new AssetDto
                {
                    Id = assetId.ToString(),
                    Name = assetState.ToString(),
                    Decimals = assetState.Precision,
                    TokenType = TokenType.FirstClassToken
                });
            }

            // Add  NEP-5 assets
            var nep5Tokens = this.GetNEP5WatchScriptHashes();
            foreach(var nep5Token in nep5Tokens)
            {
                // TODO - Issue: #150 - [AboimPinto]: Missing add the name of the token
                walletAssets.Add(new AssetDto
                {
                    Id = nep5Token.ToString(),
                    TokenType = TokenType.NEP5Token
                });
            }

            return walletAssets;
        }

        public IEnumerable<Coin> GetCoins()
        {
            // TODO - ISSUE #37 [AboimPinto]: at this point the return should not be a object from the NEO assemblies but a DTO only know by the application with only the necessary fields.

            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.GetCoins();
        }

        public IEnumerable<Coin> FindUnspentCoins()
        {
            this.ThrowIfWalletIsNotOpen();

            var coins = this.currentWallet
                .FindUnspentCoins();

            return coins;
        }

        public UInt160 GetChangeAddress()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.GetChangeAddress();
        }

        public AccountContract GetAccountContract(string accountScriptHash)
        {
            var walletAccount = this.GetWalletAccount(UInt160.Parse(accountScriptHash));

            if (walletAccount == null) return null;

            var accountContract = new AccountContract
            {
                Address = walletAccount.Contract.Address,
                ParameterList = walletAccount.Contract.ParameterList.Cast<byte>().ToArray().ToHexString(),
                ScriptHash = walletAccount.Contract.ScriptHash.ToString(),
                RedeemScriptHex = walletAccount.Contract.Script.ToHexString()

            };

            return accountContract;
        }

        public AccountKeyInfo GetAccountKeys(string accountScriptHash)
        {
            var walletAccount = this.GetWalletAccount(UInt160.Parse(accountScriptHash));

            if (walletAccount == null) return null;
            if (!walletAccount.HasKey) return null;

            var accountKeys = new AccountKeyInfo
            {
                Address = accountScriptHash,
                PublicKeyHex = walletAccount.GetKey().PublicKey.EncodePoint(true).ToHexString(),
                PrivateKeyWif = walletAccount.GetKey().PrivateKey.ToHexString()
            };

            using (walletAccount.GetKey().Decrypt())
            {
                accountKeys.PrivateKeyHex = walletAccount.GetKey().PrivateKey.ToHexString();
            }

            return accountKeys;
        }

        public Transaction GetTransaction(UInt256 hash)
        {
            return this.blockchainService.GetTransaction(hash);
        }

        public Transaction GetTransaction(UInt256 hash, out int height)
        {
            return this.blockchainService.GetTransaction(hash, out height);
        }

        public IEnumerable<string> GetVotes(string voterScriptHash)
        {
            var accountState = this.blockchainService.GetAccountState(UInt160.Parse(voterScriptHash));

            if (accountState == null)
            {
                return Enumerable.Empty<string>();
            }

            return accountState.Votes.Select(x => x.ToString());
        }

        public ContractState GetContractState(UInt160 scriptHash)
        {
            return this.blockchainService.GetContractState(scriptHash);
        }

        public AssetStateDto GetAssetState(string assetId)
        {
            var assetState = this.blockchainService.GetAssetState(UInt256.Parse(assetId));

            var assetStateDto = new AssetStateDto(
                assetState.AssetId.ToString(),
                assetState.Owner.ToString(),
                this.ScriptHashToAddress(assetState.Admin.ToString()),
                assetState.Amount == -Fixed8.Satoshi ? "+\u221e" : assetState.Amount.ToString(),
                assetState.Available.ToString(),
                true);

            return assetStateDto;
        }

        public bool CanViewCertificate(FirstClassAssetItem assetItem)
        {
            if (assetItem == null) return false;

            var queryResult = this.GetCertificateQueryResult(assetItem.AssetOwner);

            if (queryResult == null) return false;

            return queryResult.Type == CertificateQueryResultType.Good ||
                   queryResult.Type == CertificateQueryResultType.Expired ||
                   queryResult.Type == CertificateQueryResultType.Invalid;
        }

        public string ViewCertificate(FirstClassAssetItem assetItem)
        {
            return this.certificateQueryService.GetCachedCertificatePath(assetItem.AssetOwner);
        }

        public Fixed8 CalculateBonus()
        {
            return this.CalculateBonus(this.GetUnclaimedCoins().Select(p => p.Reference));
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            return this.blockchainService.CalculateBonus(inputs, ignoreClaimed);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            return this.blockchainService.CalculateBonus(inputs, heightEnd);
        }

        public Fixed8 CalculateUnavailableBonusGas(uint height)
        {
            if (!this.WalletIsOpen) return Fixed8.Zero;

            var unspent = this.FindUnspentCoins().Where(p =>p.Output.AssetId.Equals(this.blockchainService.GoverningToken.Hash)).Select(p => p.Reference);
            
            var references = new HashSet<CoinReference>();

            foreach (var group in unspent.GroupBy(p => p.PrevHash))
            {
                var transaction = this.GetTransaction(group.Key);

                if (transaction == null) continue; // not enough of the chain available

                foreach (var reference in group)
                {
                    references.Add(reference);
                }
            }

            return this.CalculateBonus(references, height);
        }

        public bool WalletContainsAccount(UInt160 scriptHash)
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.Contains(scriptHash);
        }

        public string GetNEP5TokenAvailability(string assetId)
        {
            if (!this.WalletIsOpen)
            {
                return new BigDecimal(BigInteger.Zero, 0).ToString();
            }

            return this.currentWallet.GetAvailable(UInt160.Parse(assetId)).ToString();
        }

        public string GetFirstClassTokenAvailability(string assetId)
        {
            if (!this.WalletIsOpen)
            {
                return Fixed8.Zero.ToString();
            }

            return this.currentWallet.GetAvailable(UInt256.Parse(assetId)).ToString();
        }

        public void ImportWatchOnlyAddress(string[] addressesToWatch)
        {
            foreach (var address in addressesToWatch)
            {
                if (address == null) continue;

                var trimmedAddress = address.Trim();

                if (string.IsNullOrEmpty(trimmedAddress)) continue;

                UInt160 scriptHash;
                try
                {
                    scriptHash = this.AddressToScriptHash(trimmedAddress);
                }
                catch (FormatException)
                {
                    continue;
                }

                var account = this.currentWallet.CreateAccount(scriptHash);

                this.AddAccountItem(account);
            }

            this.TrySaveWallet();
        }

        public bool DeleteAccount(AccountItem account)
        {
            this.ThrowIfWalletIsNotOpen();

            Guard.ArgumentIsNotNull(account, nameof(account));

            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            var accountScriptHash = account.ScriptHash;

            var deletedSuccessfully = this.currentWallet.DeleteAccount(UInt160.Parse(accountScriptHash));

            if (!deletedSuccessfully) return false;

            this.currentWalletInfo.RemoveAccount(UInt160.Parse(accountScriptHash));

            this.TrySaveWallet();

            this.SetWalletBalanceChangedFlag();

            return true;
        }

        public Transaction MakeTransaction(
            Transaction transaction, 
            UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction, change_address: changeAddress, fee: fee);
        }

        public string BytesToScriptHash(byte[] data)
        {
            return data.ToScriptHash().ToString();
        }

        public UInt160 AddressToScriptHash(string address)
        {
            return BaseWallet.ToScriptHash(address);
        }

        public string ScriptHashToAddress(string scriptHash)
        {
            return BaseWallet.ToAddress(UInt160.Parse(scriptHash));
        }

        public bool AddressIsValid(string address)
        {
            try
            {
                this.AddressToScriptHash(address);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public void DeleteFirstClassAsset(FirstClassAssetItem assetItem)
        {
            var value = this.GetFirstClassTokenAvailability(assetItem.AssetId);

            var transactionOutput = new TransactionOutput
            {
                AssetId = UInt256.Parse(assetItem.AssetId),
                Value = Fixed8.Parse(value),
                ScriptHash = this.RecycleScriptHash
            };

            var deleteTransaction = this.MakeTransaction(new ContractTransaction
            {
                Outputs = new[] { transactionOutput }
            }, fee: Fixed8.Zero);

            this.SignAndRelay(deleteTransaction);
        }

        public void ClaimUtilityTokenAsset()
        {
            var claims = this.GetUnclaimedCoins()
                .Select(p => p.Reference)
                    .ToArray();

            if (claims.Length == 0) return;

            var claimTransaction = new ClaimTransaction
            {
                Claims = claims,
                Attributes = new TransactionAttribute[0],
                Inputs = new CoinReference[0],
                Outputs = new[]
                {
                    new TransactionOutput
                    {
                        AssetId = this.blockchainService.UtilityToken.Hash,
                        Value = this.CalculateBonus(claims),
                        ScriptHash = this.GetChangeAddress()
                    }
                }
            };

            this.SignAndRelay(claimTransaction);
        }
        
        public void AddLockContractAccount(string publicKey, uint unlockDateTime)
        {
            using (var sb = new ScriptBuilder())
            {
                sb.EmitPush(publicKey.ToECPoint());
                sb.EmitPush(unlockDateTime);
                // Lock 2.0 in mainnet tx:4e84015258880ced0387f34842b1d96f605b9cc78b308e1f0d876933c2c9134b
                sb.EmitAppCall(UInt160.Parse("d3cce84d0800172d09c88ccad61130611bd047a4"));

                try
                {
                    var contract = Contract.Create(new[] { ContractParameterType.Signature }, sb.ToArray());
                    this.CreateAccount(contract);
                }
                catch
                {
                    this.notificationService.ShowErrorNotification(Strings.AddContractFailedMessage);
                }
            }
        }

        public IEnumerable<string> GetPublicKeysFromStandardAccounts()
        {
            return this
                .GetStandardAccounts()
                .Select(x => x.GetKey().PublicKey.ToString())
                .ToList();
        }

        public IEnumerable<string> GetAddressesForNonWatchOnlyAccounts()
        {
            return this
                .GetNonWatchOnlyAccounts()
                .Select(x => x.Address.ToString())
                .ToList();
        }

        public void AddMultiSignatureContract(int minimunSignatureNumber, IEnumerable<string> publicKeys)
        {
            var ecPoints = publicKeys
                .Select(p => p.ToECPoint())
                .ToArray();

            var contract = Contract.CreateMultiSigContract(minimunSignatureNumber, ecPoints);

            if (contract == null) return;
            this.CreateAccount(contract);
        }

        public void AddContractWithParameters(string reedemScript, string parameterList)
        {
            var parameters = parameterList.HexToBytes().Select(p => (ContractParameterType)p).ToArray();
            var scriptBytes = reedemScript.HexToBytes();

            var contract = Contract.Create(parameters, scriptBytes);

            this.CreateAccount(contract);
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.initialized)
                    {
                        this.blockchainService.BlockAdded -= this.OnBlockAdded;

                        // Stop automatic refresh timer
                        this.refreshTimer?.Stop();
                        this.refreshTimer = null;

                        // Save and dispose of wallet if required
                        if (this.WalletIsOpen)
                        {
                            this.currentWallet.BalanceChanged -= this.CurrentWalletBalanceChanged;

                            this.TrySaveWallet();

                            this.WalletDispose();
                        }
                    }

                    // Dispose of blockchain controller
                    this.blockchainService.Dispose();

                    this.disposed = true;
                }
            }
        }
        #endregion

        #region Private Methods

        private void OnBlockAdded(object sender, EventArgs e)
        {
            if (!this.WalletIsOpen) return;

            this.checkNep5Balance = true;

            var coins = this.GetCoins();

            if (coins.Any(coin => !coin.State.HasFlag(CoinState.Spent) &&
                                  coin.Output.AssetId.Equals(this.blockchainService.GoverningToken.Hash)))
            {
                this.SetWalletBalanceChangedFlag();
            }

            this.RefreshTransactionConfirmations();
        }

        private IEnumerable<UInt256> FindUnspentAssetIds()
        {
            this.ThrowIfWalletIsNotOpen();

            var distinctUnspentedCoinsAssetId = this.currentWallet
                .FindUnspentCoins()
                .Select(x => x.Output.AssetId)
                .Distinct();

            return distinctUnspentedCoinsAssetId;
        }

        /// <summary>
        /// Throws <see cref="WalletIsNotOpenException" /> if a wallet is not open.
        /// </summary>
        private void ThrowIfWalletIsNotOpen()
        {
            if (this.WalletIsOpen) return;

            throw new WalletIsNotOpenException();
        }

        private void WalletDispose()
        {
            this.currentWalletLocker?.Dispose();
            this.currentWalletLocker = null;

            var disposableWallet = this.currentWallet as IDisposable;
            disposableWallet?.Dispose();
            this.currentWallet = null;
        }

        private void Refresh()
        {
            var lockAcquired = Monitor.TryEnter(this.walletRefreshLock);

            if (!lockAcquired) return;

            try
            {
                var blockchainStatus = this.blockchainService.GetStatus();
                var networkStatus = this.nodeController.GetStatus();

                var walletStatus = new WalletStatus(this.WalletHeight, blockchainStatus, networkStatus);

                this.messagePublisher.Publish(new WalletStatusMessage(walletStatus));

                // Update wallet
                if (!this.WalletIsOpen) return;

                this.UpdateAccountBalances();

                this.UpdateFirstClassAssetBalances();

                this.UpdateNEP5TokenBalances(blockchainStatus.TimeSinceLastBlock);
            }
            finally
            {
                Monitor.Exit(this.walletRefreshLock);
            }
        }

        private void SetCurrentWallet(BaseWallet wallet, IDisposable walletLocker)
        {
            if (this.WalletIsOpen)
            {
                // Dispose current wallet
                this.currentWallet.BalanceChanged -= this.CurrentWalletBalanceChanged;

                // Try save wallet in case something was not saved
                this.TrySaveWallet();

                // Dispose of wallet if required
                this.WalletDispose();
            }
            
            this.currentWallet = wallet;
            this.currentWalletLocker = walletLocker;
            this.currentWalletInfo = new WalletInfo();

            this.messagePublisher.Publish(new CurrentWalletHasChangedMessage());

            // Setup wallet if required
            if (this.WalletIsOpen)
            {
                // Load accounts

                foreach (var account in this.GetAccounts())
                {
                    this.AddAccountItem(account);
                }

                // Load transactions
                var walletTransactionHashes = this.currentWallet.GetTransactions();

                // Get transaction information from transaction hashes
                var walletTransactions = new List<TransactionItem>();
                foreach (var transactionHash in walletTransactionHashes)
                {
                    var transaction = this.blockchainService.GetTransaction(transactionHash, out var height);

                    if (transaction == null) continue;

                    var transactionTime = this.blockchainService.GetTimeOfBlock((uint) height);

                    walletTransactions.Add(new TransactionItem(transactionHash, transaction.Type, (uint) height, transactionTime));
                }

                // Add transactions to wallet info, ordered by time
                var orderedTransactions = walletTransactions.OrderBy(item => item.Time);

                foreach (var transactionItem in orderedTransactions)
                {
                    this.AddTransaction(transactionItem);
                }

                this.currentWallet.BalanceChanged += this.CurrentWalletBalanceChanged;

                this.SetWalletBalanceChangedFlag();
                this.checkNep5Balance = true;
            }
        }

        private void TrySaveWallet()
        {
            var nep6Wallet = this.currentWallet as NEP6Wallet;
            nep6Wallet?.Save();
        }

        private void CurrentWalletBalanceChanged(object sender, BalanceEventArgs e)
        {
            var transaction = e.Transaction;

            // TODO Check this logic is correct
            var transactionHeight = e.Height ?? this.blockchainService.BlockHeight;

            var transactionItem = new TransactionItem(transaction.Hash, transaction.Type, transactionHeight, TimeHelper.UnixTimestampToDateTime(e.Time));
            
            this.AddTransaction(transactionItem);

            this.SetWalletBalanceChangedFlag();
        }

        private WalletAccount GetWalletAccount(UInt160 scriptHash)
        {
            if (scriptHash == null) return null;

            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts()
                .FirstOrDefault(account => scriptHash.Equals(account.ScriptHash));
        }

        private void AddAccountItem(WalletAccount account)
        {
            // Check if account item already exists
            if (this.currentWalletInfo.ContainsAccount(account.ScriptHash)) return;

            AccountType accountType;

            if (account.WatchOnly)
            {
                accountType = AccountType.WatchOnly;
            }
            else
            {
                accountType = account.Contract.IsStandard
                    ? AccountType.Standard
                    : AccountType.NonStandard;
            }

            var newAccountItem = new AccountItem(account.Label, account.ScriptHash.ToString(), accountType);

            this.currentWalletInfo.AddAccount(newAccountItem);

            this.messagePublisher.Publish(new AccountAddedMessage(newAccountItem));
        }

        private void UpdateAccountBalances()
        {
            var coins = this.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();

            if (!coins.Any()) return;

            var balanceNeo = coins.Where(p => p.Output.AssetId.Equals(this.blockchainService.GoverningToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
            var balanceGas = coins.Where(p => p.Output.AssetId.Equals(this.blockchainService.UtilityToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));

            var accountsList = this.currentWalletInfo
                .GetAccounts()
                .ToList();
            
            foreach (var account in accountsList)
            {
                var scriptHash = UInt160.Parse(account.ScriptHash);
                var neo = balanceNeo.ContainsKey(scriptHash) ? balanceNeo[scriptHash] : Fixed8.Zero;
                var gas = balanceGas.ContainsKey(scriptHash) ? balanceGas[scriptHash] : Fixed8.Zero;
                account.Neo = neo;
                account.Gas = gas;
            }

            // TODO Publish an AccountBalancesChangedMessage
        }

        private void UpdateFirstClassAssetBalances()
        {
            var bonus = Fixed8.Zero;

            if (this.GetWalletBalanceChangedFlag())
            {
                var coins = this.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();
                var bonusAvailable = this.blockchainService.CalculateBonus(this.GetUnclaimedCoins().Select(p => p.Reference));

                var confirmedGoverningTokens = coins
                    .Where(p => 
                        p.State.HasFlag(CoinState.Confirmed) && 
                        p.Output.AssetId.Equals(this.blockchainService.GoverningToken.Hash))
                    .Select(p => p.Reference);

                // TODO Issue #158 [AboimPinto]: Calculation of the Bonus is returning an Error
                //if (!this.WalletIsSynchronized)
                //{
                //    var lastBlockHeight = this.blockchainController.BlockHeight + 1;

                //    var bonusUnavailable = this.blockchainController.CalculateBonus(confirmedGoverningTokens, lastBlockHeight);
                //    bonus = bonusAvailable + bonusUnavailable;
                //}

                var assetDictionary = coins.GroupBy(p => p.Output.AssetId, (k, g) => new
                {
                    Asset = this.blockchainService.GetAssetState(k),
                    Value = g.Sum(p => p.Output.Value),
                    Claim = k.Equals(this.blockchainService.UtilityToken.Hash) ? bonus : Fixed8.Zero
                }).ToDictionary(p => p.Asset.AssetId);

                if (bonus != Fixed8.Zero && !assetDictionary.ContainsKey(this.blockchainService.UtilityToken.Hash))
                {
                    assetDictionary[this.blockchainService.UtilityToken.Hash] = new
                    {
                        Asset = this.blockchainService.GetAssetState(this.blockchainService.UtilityToken.Hash),
                        Value = Fixed8.Zero,
                        Claim = bonus
                    };
                }

                foreach (var asset in this.currentWalletInfo.GetFirstClassAssets())
                {
                    if (assetDictionary.ContainsKey(UInt256.Parse(asset.AssetId))) continue;

                    this.currentWalletInfo.RemoveAsset(asset);
                }

                foreach (var asset in assetDictionary.Values)
                {
                    if (asset.Asset == null || asset.Asset.AssetId == null) continue;

                    var valueText = asset.Value.ToString();

                    if (asset.Asset.AssetId.Equals(this.blockchainService.UtilityToken.Hash))
                    {
                        valueText += $"+({asset.Claim})";
                    }

                    var item = this.currentWalletInfo.GetFirstClassAsset(asset.Asset.AssetId);

                    if (item != null)
                    {
                        // TODO Update balance of existing asset item
                    }
                    else
                    {
                        // Add new asset item
                        string assetName;
                        switch (asset.Asset.AssetType)
                        {
                            case AssetType.GoverningToken:
                                assetName = "NEO";
                                break;

                            case AssetType.UtilityToken:
                                assetName = "NeoGas";
                                break;

                            default:
                                assetName = asset.Asset.GetName();
                                break;
                        }

                        var assetItem = new FirstClassAssetItem(
                            asset.Asset.AssetId.ToString(),
                            asset.Asset.Owner,
                            asset.Asset.AssetType,
                            valueText)
                        {
                            Name = assetName,
                            Issuer = $"{Strings.UnknownIssuer}[{asset.Asset.Owner}]"
                        };

                        this.currentWalletInfo.AddAsset(assetItem);
                        this.messagePublisher.Publish(new AssetAddedMessage(assetItem));
                    }
                }

                this.ClearWalletBalanceChangedFlag();

                // TODO Publish a FirstClassAssetsBalancesChangedMessage
            }

            this.CheckFirstClassAssetIssuerCertificates();
        }

        private void UpdateNEP5TokenBalances(TimeSpan timeSinceLastBlock)
        {
            if (!checkNep5Balance) return;

            if (timeSinceLastBlock <= TimeSpan.FromSeconds(2)) return;

            // Update balances
            var accountScriptHashes = this.currentWalletInfo
                .GetAccounts()
                .Select(account => UInt160.Parse(account.ScriptHash))
                .ToList();

            foreach (var nep5ScriptHash in this.nep5WatchScriptHashes)
            {
                var assetItem = this.nep5QueryService.GetTotalBalance(nep5ScriptHash, accountScriptHashes);

                if (assetItem == null) continue;

                var item = this.currentWalletInfo.GetNEP5Asset(nep5ScriptHash);

                if (item != null)
                {
                    if (assetItem.BalanceIsZero)
                    {
                        // TODO If the current balance is zero, remove asset item from collection
                    }
                    else
                    {
                        // TODO Update balance of existing asset item
                    }
                }
                else
                {
                    // Do not add item if it has a balance of zero
                    if (!assetItem.BalanceIsZero)
                    {
                        this.currentWalletInfo.AddAsset(assetItem);

                        this.messagePublisher.Publish(new AssetAddedMessage(assetItem));
                    }
                }
            }

            // TODO Publish a NEP5AssetBalancesChangedMessage

            checkNep5Balance = false;
        }

        private void AddTransaction(TransactionItem transaction)
        {
            var transactionNotAdded = !this.currentWalletInfo.GetTransactions().Any(x => x.Hash == transaction.Hash);

            if (transactionNotAdded)
            {
                this.currentWalletInfo.AddTransaction(transaction);
                this.messagePublisher.Publish(new TransactionAddedMessage(transaction));
            }
        }

        private void CheckFirstClassAssetIssuerCertificates()
        {
            foreach (var asset in this.currentWalletInfo.GetFirstClassAssets()
                .Where(item => !item.IssuerCertificateChecked))
            {
                if (asset.AssetOwner == null) continue;

                var queryResult = this.GetCertificateQueryResult(asset.AssetOwner);

                if (queryResult == null) continue;

                using (queryResult)
                {
                    asset.SetIssuerCertificateQueryResult(queryResult.Type, queryResult.Certificate?.Subject);
                }
            }
        }

        private void RefreshTransactionConfirmations()
        {
            var blockHeight = this.blockchainService.BlockHeight;

            this.currentWalletInfo.UpdateTransactionConfirmations(blockHeight);
            
            this.messagePublisher.Publish(new TransactionConfirmationsUpdatedMessage(blockHeight));
        }

        private bool GetWalletBalanceChangedFlag()
        {
            return this.balanceChanged;
        }

        private void SetWalletBalanceChangedFlag()
        {
            this.balanceChanged = true;
        }

        private void ClearWalletBalanceChangedFlag()
        {
            this.balanceChanged = false;
        }

        private CertificateQueryResult GetCertificateQueryResult(ECPoint publicKey)
        {
            // Check if certificate has been cached from a previous query
            if (this.certificateQueryResultCache.ContainsKey(publicKey))
            {
                return this.certificateQueryResultCache[publicKey];
            }

            // Query for certificate
            var result = this.certificateQueryService.Query(publicKey);

            if (result == null) return null;

            // Cache certificate query result
            this.certificateQueryResultCache.Add(publicKey, result);

            return result;
        }

        private IEnumerable<Coin> GetUnclaimedCoins()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.GetUnclaimedCoins();
        }

        private Transaction BuildTransaction<TParameters>(TParameters parameters) where TParameters : TransactionParameters
        {
            var builder = this.transactionBuilderFactory.GetBuilder(parameters);

            var transaction = builder.Build(parameters);

            if (transaction is InvocationTransaction invocationTransaction)
            {
                var transactionFee = invocationTransaction.Gas.Equals(Fixed8.Zero) ? NetworkFee : Fixed8.Zero;

                transaction = this.MakeTransaction(new InvocationTransaction
                {
                    Version = transaction.Version,
                    Script = invocationTransaction.Script,
                    Gas = invocationTransaction.Gas,
                    Attributes = transaction.Attributes,
                    Inputs = transaction.Inputs,
                    Outputs = transaction.Outputs
                }, fee: transactionFee);

                if (transaction == null)
                {
                    throw new Exception("Transaction could not be created!");
                }
            }
            else if (transaction is IssueTransaction issueTransaction)
            {
                // Set asset distribution transaction fee
                transaction = this.currentWallet.MakeTransaction(issueTransaction, fee: Fixed8.One);
            }
            else
            {
                var transferParameters = parameters as AssetTransferTransactionParameters;

                if (transferParameters != null && transaction is ContractTransaction contractTransaction)
                {
                    // A first-class asset transfer transaction is being built
                    // Add fee and change address info to the transaction
                    var fee = Fixed8.Zero;
                    if (!string.IsNullOrEmpty(transferParameters.TransferFee))
                    {
                        fee = Fixed8.Parse(transferParameters.TransferFee);
                    }

                    UInt160 changeAddress = null;
                    if (!string.IsNullOrEmpty(transferParameters.TransferChangeAddress))
                    {
                        changeAddress = BaseWallet.ToScriptHash(transferParameters.TransferChangeAddress);
                    }

                    transaction = this.MakeTransaction(contractTransaction, changeAddress, fee);
                }
            }

            return transaction;
        }

        private static string GetInvocationTestResult(ApplicationEngine applicationEngine)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"VM State: {applicationEngine.State}");
            builder.AppendLine($"Gas Consumed: {applicationEngine.GasConsumed}");
            builder.AppendLine($"Evaluation Stack: {new JArray(applicationEngine.EvaluationStack.Select(p => p.ToParameter().ToJson()))}");

            return builder.ToString();
        }
        #endregion
    }
}
