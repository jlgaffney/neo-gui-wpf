using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Neo.Core;
using Neo.Gui.Globalization.Resources;
using Neo.Implementations.Wallets.NEP6;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Core.Certificates;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;
using Neo.UI.Core.Exceptions;
using Neo.UI.Core.Extensions;
using Neo.UI.Core.Helpers;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Messages;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Status;
using Neo.VM;
using Neo.Wallets;
using DeprecatedWallet = Neo.Implementations.Wallets.EntityFramework.UserWallet;
using ECPoint = Neo.Cryptography.ECC.ECPoint;
using Timer = System.Timers.Timer;

namespace Neo.UI.Core.Controllers.Implementations
{
    internal class WalletController :
        IWalletController,
        IMessageHandler<BlockAddedMessage>
    {
        #region Private Fields 
        private readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly IBlockchainController blockchainController;
        private readonly ICertificateService certificateService;
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly INetworkController networkController;
        private readonly INotificationService notificationService;

        private readonly string blockchainDataDirectoryPath;

        private readonly int localNodePort;
        private readonly int localWSPort;

        private readonly string certificateCachePath;

        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        private readonly object walletRefreshLock = new object();

        private bool initialized;
        private bool disposed;

        private Timer refreshTimer;

        private Wallet currentWallet;
        private IDisposable currentWalletLocker;
        private WalletInfo currentWalletInfo;

        private bool balanceChanged;
        private bool checkNep5Balance;

        private UInt160[] nep5WatchScriptHashes;

        #endregion

        #region Constructor 
        public WalletController(
            IBlockchainController blockchainController,
            ICertificateService certificateService,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber,
            INetworkController networkController,
            INotificationService notificationService,
            ISettingsManager settingsManager)
        {
            this.blockchainController = blockchainController;
            this.certificateService = certificateService;
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;
            this.networkController = networkController;
            this.notificationService = notificationService;

            this.blockchainDataDirectoryPath = settingsManager.BlockchainDataDirectoryPath;

            this.localNodePort = settingsManager.LocalNodePort;
            this.localWSPort = settingsManager.LocalWSPort;

            this.certificateCachePath = settingsManager.CertificateCachePath;

            this.certificateQueryResultCache = new Dictionary<ECPoint, CertificateQueryResult>();
        }
        #endregion

        #region IWalletController implementation 
        public Fixed8 NetworkFee => Fixed8.FromDecimal(0.001m);

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

            this.networkController.Initialize(this.localNodePort, this.localWSPort);
            this.blockchainController.Initialize(this.blockchainDataDirectoryPath);
            this.certificateService.Initialize(this.certificateCachePath);

            this.messageSubscriber.Subscribe(this);

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

        public bool WalletIsSynchronized => this.WalletHeight > this.blockchainController.BlockHeight + 1;

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
            Wallet wallet;
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

        public bool Sign(ContractParametersContext context)
        {
            Guard.ArgumentIsNotNull(context, nameof(context));

            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.Sign(context);
        }

        public void Relay(Transaction transaction, bool saveTransaction = true)
        {
            Guard.ArgumentIsNotNull(transaction, nameof(transaction));

            this.networkController.Relay(transaction);

            if (!saveTransaction) return;

            this.currentWallet.ApplyTransaction(transaction);
        }

        public void Relay(IInventory inventory)
        {
            Guard.ArgumentIsNotNull(inventory, nameof(inventory));

            this.networkController.Relay(inventory);
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

        public IEnumerable<WalletAccount> GetNonWatchOnlyAccounts()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts().Where(account => !account.WatchOnly);
        }

        public IEnumerable<WalletAccount> GetStandardAccounts()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts().Where(account =>
                !account.WatchOnly && account.Contract.IsStandard);
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

            return this.currentWallet.FindUnspentCoins();
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

        public KeyPair GetAccountKey(UInt160 accountScriptHash)
        {
            var walletAccount = this.GetWalletAccount(accountScriptHash);

            if (walletAccount == null) return null;

            return walletAccount.HasKey ? walletAccount.GetKey() : null;
        }

        public Transaction GetTransaction(UInt256 hash)
        {
            return this.blockchainController.GetTransaction(hash);
        }

        public Transaction GetTransaction(UInt256 hash, out int height)
        {
            return this.blockchainController.GetTransaction(hash, out height);
        }

        public IEnumerable<ECPoint> GetVotes(UInt160 scriptHash)
        {
            var accountState = this.blockchainController.GetAccountState(scriptHash);

            if (accountState == null)
            {
                return Enumerable.Empty<ECPoint>();
            }

            return accountState.Votes;
        }

        public ContractState GetContractState(UInt160 scriptHash)
        {
            return this.blockchainController.GetContractState(scriptHash);
        }

        public AssetState GetAssetState(UInt256 assetId)
        {
            return this.blockchainController.GetAssetState(assetId);
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
            return this.certificateService.GetCachedCertificatePath(assetItem.AssetOwner);
        }

        public Fixed8 CalculateBonus()
        {
            return this.CalculateBonus(this.GetUnclaimedCoins().Select(p => p.Reference));
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            return this.blockchainController.CalculateBonus(inputs, ignoreClaimed);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            return this.blockchainController.CalculateBonus(inputs, heightEnd);
        }

        public Fixed8 CalculateUnavailableBonusGas(uint height)
        {
            if (!this.WalletIsOpen) return Fixed8.Zero;

            var unspent = this.FindUnspentCoins().Where(p =>p.Output.AssetId.Equals(this.blockchainController.GoverningToken.Hash)).Select(p => p.Reference);
            
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

        public BigDecimal GetAvailable(UInt160 assetId)
        {
            if (!this.WalletIsOpen)
            {
                return new BigDecimal(BigInteger.Zero, 0);
            }

            return this.currentWallet.GetAvailable(assetId);
        }

        public Fixed8 GetAvailable(UInt256 assetId)
        {
            if (!this.WalletIsOpen)
            {
                return Fixed8.Zero;
            }

            return this.currentWallet.GetAvailable(assetId);
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

            var deletedSuccessfully = this.currentWallet.DeleteAccount(accountScriptHash);

            if (!deletedSuccessfully) return false;

            this.currentWalletInfo.RemoveAccount(accountScriptHash);

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

            return this.currentWallet.MakeTransaction(transaction, changeAddress, fee);
        }

        public Transaction MakeTransferTransaction(IEnumerable<TransferOutput> items, string remark, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8))
        {
            var accountAddresses = this.GetAccounts().Select(p => p.ScriptHash);

            var tx = TransactionHelper.MakeTransferTransaction(items, accountAddresses, remark, changeAddress, fee);

            if (tx is ContractTransaction ctx)
            {
                tx = this.MakeTransaction(ctx, changeAddress, fee);
            }

            return tx;
        }

        public InvocationTransaction MakeValidatorRegistrationTransaction(ECPoint publicKey)
        {
            return TransactionHelper.MakeValidatorRegistrationTransaction(publicKey);
        }

        public InvocationTransaction MakeAssetCreationTransaction(
            AssetType? assetType, 
            string assetName,
            Fixed8 amount, 
            byte precision, 
            ECPoint assetOwner, 
            UInt160 assetAdmin, 
            UInt160 assetIssuer)
        {
            return TransactionHelper.MakeAssetCreationTransaction(
                assetType, assetName, amount, precision, 
                    assetOwner, assetAdmin, assetIssuer);
        }

        public InvocationTransaction MakeContractCreationTransaction(
            byte[] script, 
            byte[] parameterList, 
            ContractParameterType returnType,
            bool needsStorage, 
            string name, 
            string version, 
            string author, 
            string email, 
            string description)
        {
            return TransactionHelper.MakeContractCreationTransaction(
                script, parameterList, returnType, needsStorage,
                    name, version, author, email, description);
        }

        public UInt160 AddressToScriptHash(string address)
        {
            return Wallet.ToScriptHash(address);
        }

        public string ScriptHashToAddress(UInt160 scriptHash)
        {
            return Wallet.ToAddress(scriptHash);
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
            var value = this.GetAvailable(assetItem.AssetId);

            var transactionOutput = new TransactionOutput
            {
                AssetId = assetItem.AssetId,
                Value = value,
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

            var claimTransaction = TransactionHelper.MakeClaimTransaction(
                claims, this.blockchainController.UtilityToken.Hash, 
                    this.CalculateBonus(claims), this.GetChangeAddress());

            this.SignAndRelay(claimTransaction);
        }

        public void IssueAsset(UInt256 assetId, IEnumerable<TransferOutput> items)
        {
            this.ThrowIfWalletIsNotOpen();

            var issueTransaction = this.currentWallet.MakeTransaction(new IssueTransaction
            {
                Version = 1,
                Outputs = items.GroupBy(p => p.ScriptHash).Select(g => new TransactionOutput
                {
                    AssetId = assetId,
                    Value = g.Sum(p => new Fixed8((long)p.Value.Value)),
                    ScriptHash = g.Key
                }).ToArray()
            }, fee: Fixed8.One);

            this.SignAndRelay(issueTransaction);
        }

        public void InvokeContract(InvocationTransaction transaction)
        {
            var transactionFee = transaction.Gas.Equals(Fixed8.Zero) ? NetworkFee : Fixed8.Zero;

            var transactionWithFee = this.MakeTransaction(new InvocationTransaction
            {
                Version = transaction.Version,
                Script = transaction.Script,
                Gas = transaction.Gas,
                Attributes = transaction.Attributes,
                Inputs = transaction.Inputs,
                Outputs = transaction.Outputs
            }, fee: transactionFee);

            this.SignAndRelay(transactionWithFee);
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
            var scriptHash = reedemScript.HexToBytes();

            var contract = Contract.Create(parameters, scriptHash);

            this.CreateAccount(contract);
        }
        #endregion

        #region IMessageHandler implementation
        public void HandleMessage(BlockAddedMessage message)
        {
            if (!this.WalletIsOpen) return;

            this.checkNep5Balance = true;

            var coins = this.GetCoins();

            if (coins.Any(coin => !coin.State.HasFlag(CoinState.Spent) &&
                coin.Output.AssetId.Equals(this.blockchainController.GoverningToken.Hash)))
            {
                this.SetWalletBalanceChangedFlag();
            }

            this.RefreshTransactionConfirmations();
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
                    this.messageSubscriber.Unsubscribe(this);

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

                    // Dispose of blockchain controller
                    this.blockchainController.Dispose();

                    this.disposed = true;
                }
            }
        }
        #endregion

        #region Private Methods
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
                var blockchainStatus = this.blockchainController.GetStatus();
                var networkStatus = this.networkController.GetStatus();

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

        private void SetCurrentWallet(Wallet wallet, IDisposable walletLocker)
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
                    var transaction = this.blockchainController.GetTransaction(transactionHash, out var height);

                    if (transaction == null) continue;

                    var transactionTime = this.blockchainController.GetTimeOfBlock((uint) height);

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
            var transactionHeight = e.Height ?? this.blockchainController.BlockHeight;

            var transactionItem = new TransactionItem(transaction.Hash, transaction.Type, transactionHeight, TimeHelper.UnixTimestampToDateTime(e.Time));
            
            this.AddTransaction(transactionItem);

            this.SetWalletBalanceChangedFlag();
        }

        private WalletAccount GetWalletAccount(UInt160 scriptHash)
        {
            if (scriptHash == null) return null;

            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts().FirstOrDefault(account =>
                scriptHash.Equals(account.ScriptHash));
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

            var newAccountItem = new AccountItem(account.Label, account.ScriptHash, accountType);

            this.currentWalletInfo.AddAccount(newAccountItem);

            this.messagePublisher.Publish(new AccountAddedMessage(newAccountItem));
        }

        private void UpdateAccountBalances()
        {
            var coins = this.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();

            if (!coins.Any()) return;

            var balanceNeo = coins.Where(p => p.Output.AssetId.Equals(this.blockchainController.GoverningToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
            var balanceGas = coins.Where(p => p.Output.AssetId.Equals(this.blockchainController.UtilityToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));

            var accountsList = this.currentWalletInfo.GetAccounts().ToList();

            foreach (var account in accountsList)
            {
                var scriptHash = account.ScriptHash;
                var neo = balanceNeo.ContainsKey(scriptHash) ? balanceNeo[scriptHash] : Fixed8.Zero;
                var gas = balanceGas.ContainsKey(scriptHash) ? balanceGas[scriptHash] : Fixed8.Zero;
                account.Neo = neo;
                account.Gas = gas;
            }

            // TODO Publish an AccountBalancesChangedMessage
        }

        private void UpdateFirstClassAssetBalances()
        {
            if (this.WalletIsSynchronized) return;

            if (this.GetWalletBalanceChangedFlag())
            {
                var coins = this.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();
                var bonusAvailable = this.blockchainController.CalculateBonus(this.GetUnclaimedCoins().Select(p => p.Reference));
                var bonusUnavailable = this.blockchainController.CalculateBonus(coins.Where(p => p.State.HasFlag(CoinState.Confirmed) && p.Output.AssetId.Equals(this.blockchainController.GoverningToken.Hash)).Select(p => p.Reference), this.blockchainController.BlockHeight + 1);
                var bonus = bonusAvailable + bonusUnavailable;

                var assetDictionary = coins.GroupBy(p => p.Output.AssetId, (k, g) => new
                {
                    Asset = this.blockchainController.GetAssetState(k),
                    Value = g.Sum(p => p.Output.Value),
                    Claim = k.Equals(this.blockchainController.UtilityToken.Hash) ? bonus : Fixed8.Zero
                }).ToDictionary(p => p.Asset.AssetId);

                if (bonus != Fixed8.Zero && !assetDictionary.ContainsKey(this.blockchainController.UtilityToken.Hash))
                {
                    assetDictionary[this.blockchainController.UtilityToken.Hash] = new
                    {
                        Asset = this.blockchainController.GetAssetState(this.blockchainController.UtilityToken.Hash),
                        Value = Fixed8.Zero,
                        Claim = bonus
                    };
                }

                foreach (var asset in this.currentWalletInfo.GetFirstClassAssets())
                {
                    if (assetDictionary.ContainsKey(asset.AssetId)) continue;

                    this.currentWalletInfo.RemoveAsset(asset);
                }

                foreach (var asset in assetDictionary.Values)
                {
                    if (asset.Asset == null || asset.Asset.AssetId == null) continue;

                    var valueText = asset.Value.ToString();

                    if (asset.Asset.AssetId.Equals(this.blockchainController.UtilityToken.Hash))
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
                            asset.Asset.AssetId,
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
            var accountScriptHashes = this.currentWalletInfo.GetAccounts().Select(account => account.ScriptHash).ToList();

            foreach (var nep5ScriptHash in this.nep5WatchScriptHashes)
            {
                var assetItem = NEP5Helper.GetBalance(nep5ScriptHash, accountScriptHashes);

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
            this.currentWalletInfo.AddTransaction(transaction);
            
            this.messagePublisher.Publish(new TransactionAddedMessage(transaction));
        }

        private void CheckFirstClassAssetIssuerCertificates()
        {
            foreach (var asset in this.currentWalletInfo.GetFirstClassAssets()
                .Where(item => !item.IssuerCertificateChecked))
            {
                if (asset.AssetOwner == null) continue;

                var queryResult = this.GetCertificateQueryResult(asset.AssetOwner);

                if (queryResult == null) continue;

                asset.SetIssuerCertificateQueryResult(queryResult);
            }
        }

        private void RefreshTransactionConfirmations()
        {
            var blockHeight = this.blockchainController.BlockHeight;

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
            var result = this.certificateService.Query(publicKey);

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
        #endregion
    }
}
