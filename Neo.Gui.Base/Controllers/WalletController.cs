using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Implementations.Wallets.NEP6;
using Neo.Network;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;

using Neo.Gui.Base.Certificates;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Exceptions;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Base.Helpers;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Services;
using Neo.Gui.Base.Status;

using CryptographicException = System.Security.Cryptography.CryptographicException;
using DeprecatedWallet = Neo.Implementations.Wallets.EntityFramework.UserWallet;
using Timer = System.Timers.Timer;

namespace Neo.Gui.Base.Controllers
{
    internal class WalletController :
        IWalletController,
        IMessageHandler<AddContractsMessage>,
        IMessageHandler<AddContractMessage>,
        IMessageHandler<ImportPrivateKeyMessage>,
        IMessageHandler<ImportCertificateMessage>,
        IMessageHandler<SignTransactionAndShowInformationMessage>,
        IMessageHandler<BlockAddedMessage>
    {
        #region Private Fields 
        private readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly IBlockchainController blockchainController;
        private readonly ICertificateService certificateService;
        private readonly INetworkController networkController;
        private readonly INotificationService notificationService;
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;
        
        private readonly int localNodePort;
        private readonly int localWSPort;

        private readonly string blockchainDataDirectoryPath;

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
            INetworkController networkController,
            INotificationService notificationService,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber,
            ISettingsManager settingsManager)
        {
            this.blockchainController = blockchainController;
            this.certificateService = certificateService;
            this.networkController = networkController;
            this.notificationService = notificationService;
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;

            this.blockchainDataDirectoryPath = settingsManager.BlockchainDataDirectoryPath;

            this.localNodePort = settingsManager.LocalNodePort;
            this.localWSPort = settingsManager.LocalWSPort;

            this.certificateCachePath = settingsManager.CertificateCachePath;

            this.messageSubscriber.Subscribe(this);

            this.certificateQueryResultCache = new Dictionary<ECPoint, CertificateQueryResult>();
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
                throw new Exception(nameof(IWalletController) + " has already been initialized!");
            }

            this.networkController.Initialize(this.localNodePort, this.localWSPort);
            this.blockchainController.Initialize(this.blockchainDataDirectoryPath);
            this.certificateService.Initialize(this.certificateCachePath);

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

        public string MigrateWallet(string walletPath, string password, string newWalletPath = null)
        {
            if (string.IsNullOrEmpty(newWalletPath))
            {
                newWalletPath = Path.ChangeExtension(walletPath, ".json");
                newWalletPath = FileManager.GetAvailableFilePath(newWalletPath);
            }

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
                this.CreateNewAccount();
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

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            this.ThrowIfWalletIsNotOpen();
            
            return false;//this.currentWallet.ChangePassword(oldPassword, newPassword);
        }

        public void CreateNewAccount()
        {
            this.ThrowIfWalletIsNotOpen();

            var account = this.currentWallet.CreateAccount();

            this.AddAccountItem(account);

            var nep6Wallet = this.currentWallet as NEP6Wallet;
            nep6Wallet?.Save();
        }

        public bool Sign(ContractParametersContext context)
        {
            return this.currentWallet.Sign(context);
        }

        public void Relay(Transaction transaction, bool saveTransaction = true)
        {
            this.networkController.Relay(transaction);

            if (saveTransaction)
            {
                this.currentWallet.ApplyTransaction(transaction);
            }
        }

        public void Relay(IInventory inventory)
        {
            this.networkController.Relay(inventory);
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

        public IEnumerable<Coin> GetUnclaimedCoins()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.GetUnclaimedCoins();
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

        public Contract GetAccountContract(UInt160 accountScriptHash)
        {
            var walletAccount = this.GetWalletAccount(accountScriptHash);

            return walletAccount?.Contract;
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

        public AccountState GetAccountState(UInt160 scriptHash)
        {
            return this.blockchainController.GetAccountState(scriptHash);
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

        public BigDecimal GetAvailable(UIntBase assetId)
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

        public void ImportWatchOnlyAddress(string addressToImport)
        {
            using (var reader = new StringReader(addressToImport))
            {
                while (true)
                {
                    var address = reader.ReadLine();

                    if (address == null) break;

                    address = address.Trim();

                    if (string.IsNullOrEmpty(address)) continue;

                    UInt160 scriptHash;
                    try
                    {
                        scriptHash = Wallet.ToScriptHash(address);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }

                    var account = this.currentWallet.CreateAccount(scriptHash);

                    this.AddAccountItem(account);
                }

                var nep6Wallet = this.currentWallet as NEP6Wallet;
                nep6Wallet?.Save();
            }
        }

        public bool DeleteAccount(AccountItem account)
        {
            this.ThrowIfWalletIsNotOpen();

            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            var accountScriptHash = account.ScriptHash;

            var deletedSuccessfully = this.currentWallet.DeleteAccount(accountScriptHash);

            if (!deletedSuccessfully) return false;

            this.currentWalletInfo.RemoveAccount(accountScriptHash);

            this.SetWalletBalanceChangedFlag();

            return true;
        }

        public Transaction MakeTransaction(
            Transaction transaction, 
            UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction);
        }

        public ContractTransaction MakeTransaction(
            ContractTransaction transaction, 
            UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction, changeAddress, fee);
        }

        public InvocationTransaction MakeTransaction(
            InvocationTransaction transaction, 
            UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction, changeAddress, fee);
        }

        public IssueTransaction MakeTransaction(
            IssueTransaction transaction, 
            UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction, changeAddress, fee);
        }

        public Transaction MakeClaimTransaction(CoinReference[] claims)
        {
            return new ClaimTransaction
            {
                Claims = claims,
                Attributes = new TransactionAttribute[0],
                Inputs = new CoinReference[0],
                Outputs = new[]
                {
                    new TransactionOutput
                    {
                        AssetId = this.blockchainController.UtilityToken.Hash,
                        Value = this.CalculateBonus(claims),
                        ScriptHash = this.GetChangeAddress()
                    }
                }
            };
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

        public UInt160 ToScriptHash(string address)
        {
            return Wallet.ToScriptHash(address);
        }

        public string ToAddress(UInt160 scriptHash)
        {
            return Wallet.ToAddress(scriptHash);
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

            this.SignAndRelayTransaction(deleteTransaction);

        }
        #endregion

        #region IMessageHandler implementation 
        public void HandleMessage(AddContractsMessage message)
        {
            if (message.Contracts == null || !message.Contracts.Any()) return;

            foreach (var contract in message.Contracts)
            {
                var account = this.currentWallet.CreateAccount(contract);

                this.AddAccountItem(account);
            }

            var nep6Wallet = this.currentWallet as NEP6Wallet;
            nep6Wallet?.Save();
        }

        public void HandleMessage(AddContractMessage message)
        {
            this.ThrowIfWalletIsNotOpen();

            if (message.Contract == null) return;

            var account = this.currentWallet.CreateAccount(message.Contract);

            this.AddAccountItem(account);

            var nep6Wallet = this.currentWallet as NEP6Wallet;
            nep6Wallet?.Save();
        }

        public void HandleMessage(ImportPrivateKeyMessage message)
        {
            if (message.WifStrings == null) return;

            if (!message.WifStrings.Any()) return;

            foreach (var wif in message.WifStrings)
            {
                WalletAccount account;
                try
                {
                    account = this.currentWallet.Import(wif);
                }
                catch (FormatException)
                {
                    // Skip WIF line
                    continue;
                }

                this.AddAccountItem(account);
            }
            
            var nep6Wallet = this.currentWallet as NEP6Wallet;
            nep6Wallet?.Save();
        }

        public void HandleMessage(ImportCertificateMessage message)
        {
            if (message.SelectedCertificate == null) return;

            WalletAccount account;
            try
            {
                account = this.currentWallet.Import(message.SelectedCertificate);
            }
            catch
            {
                // TODO Localise this text
                this.notificationService.ShowErrorNotification("Certificate import failed!");
                return;
            }

            this.AddAccountItem(account);

            var nep6Wallet = this.currentWallet as NEP6Wallet;
            nep6Wallet?.Save();
        }

        public void HandleMessage(SignTransactionAndShowInformationMessage message)
        {
            this.SignAndRelayTransaction(message.Transaction);
        }

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

                        var nep6Wallet = this.currentWallet as NEP6Wallet;
                        nep6Wallet?.Save();

                        this.currentWalletLocker?.Dispose();

                        var disposableWallet = this.currentWallet as IDisposable;
                        disposableWallet?.Dispose();
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

                // Save NEP-6 wallet just in case something was not saved
                var nep6Wallet = this.currentWallet as NEP6Wallet;
                nep6Wallet?.Save();

                // Dispose of wallet if required
                var disposableWallet = this.currentWallet as IDisposable;
                disposableWallet?.Dispose();
            }

            this.messagePublisher.Publish(new ClearAccountsMessage());
            this.messagePublisher.Publish(new ClearAssetsMessage());
            this.messagePublisher.Publish(new ClearTransactionsMessage());

            this.currentWallet = wallet;
            this.currentWalletLocker = walletLocker;
            this.currentWalletInfo = new WalletInfo();

            // Setup wallet if required
            if (this.WalletIsOpen)
            {
                // Load accounts

                foreach (var account in this.GetAccounts())
                {
                    this.AddAccountItem(account);
                }

                // Load transactions
                var walletTransactions = this.currentWallet.GetTransactions();

                foreach (var i in walletTransactions.Select(p => new
                {
                    Transaction = this.blockchainController.GetTransaction(p, out int height),
                    Height = (uint)height
                }).Where(p => p.Transaction != null).Select(p => new

                {
                    p.Transaction,
                    p.Height,
                    Time = this.blockchainController.GetTimeOfBlock(p.Height)
                }).OrderBy(p => p.Time))
                {
                    this.AddTransaction(i.Transaction, i.Height, i.Time);
                }

                this.currentWallet.BalanceChanged += this.CurrentWalletBalanceChanged;

                this.SetWalletBalanceChangedFlag();
                this.checkNep5Balance = true;
            }

            this.messagePublisher.Publish(new CurrentWalletHasChangedMessage());
        }

        private void CurrentWalletBalanceChanged(object sender, BalanceEventArgs e)
        {
            // TODO Check this logic is correct
            var transactionHeight = e.Height ?? this.blockchainController.BlockHeight;

            this.AddTransaction(e.Transaction, transactionHeight, TimeHelper.UnixTimestampToDateTime(e.Time));

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

                    var valueText = asset.Value + (asset.Asset.AssetId.Equals(this.blockchainController.UtilityToken.Hash) ? $"+({asset.Claim})" : "");

                    var item = this.currentWalletInfo.GetFirstClassAsset(asset.Asset.AssetId);

                    if (item != null)
                    {
                        // Asset item already exists
                        item.Value = valueText;
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

                        var assetItem = new FirstClassAssetItem(asset.Asset.AssetId, asset.Asset.Owner, asset.Asset.AssetType)
                        {
                            Name = assetName,
                            Issuer = $"{Strings.UnknownIssuer}[{asset.Asset.Owner}]",
                            Value = valueText
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
                byte[] script;
                using (var builder = new ScriptBuilder())
                {
                    foreach (var accountScriptHash in accountScriptHashes)
                    {
                        builder.EmitAppCall(nep5ScriptHash, "balanceOf", accountScriptHash);
                    }
                    builder.Emit(OpCode.DEPTH, OpCode.PACK);
                    builder.EmitAppCall(nep5ScriptHash, "decimals");
                    builder.EmitAppCall(nep5ScriptHash, "name");
                    script = builder.ToArray();
                }

                var engine = ApplicationEngine.Run(script);
                if (engine.State.HasFlag(VMState.FAULT)) continue;

                var name = engine.EvaluationStack.Pop().GetString();
                var decimals = (byte)engine.EvaluationStack.Pop().GetBigInteger();
                var amount = engine.EvaluationStack.Pop().GetArray().Aggregate(BigInteger.Zero, (x, y) => x + y.GetBigInteger());
                if (amount == 0) continue;
                var balance = new BigDecimal(amount, decimals);
                var valueText = balance.ToString();

                var item = this.currentWalletInfo.GetNEP5Asset(nep5ScriptHash);

                if (item != null)
                {
                    item.Value = valueText;
                }
                else
                {
                    var assetItem = new NEP5AssetItem(nep5ScriptHash)
                    {
                        Name = name,
                        Value = valueText
                    };

                    this.currentWalletInfo.AddAsset(assetItem);

                    this.messagePublisher.Publish(new AssetAddedMessage(assetItem));
                }
            }

            // TODO Publish a NEP5AssetBalancesChangedMessage

            checkNep5Balance = false;
        }

        private void AddTransaction(Transaction transaction, uint blockHeight, DateTime transactionTime)
        {
            var transactionItem = new TransactionItem(transaction, blockHeight, transactionTime);

            this.currentWalletInfo.AddTransaction(transactionItem);

            // TODO Replace with a TransactionAddedMessage
            this.messagePublisher.Publish(new TransactionsHaveChangedMessage(this.currentWalletInfo.GetTransactions()));
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
            this.currentWalletInfo.UpdateTransactionConfirmations(this.blockchainController.BlockHeight);

            // TODO Replace with a TransactionConfirmationsUpdatedMessage
            this.messagePublisher.Publish(new TransactionsHaveChangedMessage(this.currentWalletInfo.GetTransactions()));
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

        private void SignAndRelayTransaction(Transaction transaction)
        {
            if (transaction == null)
            {
                this.notificationService.ShowErrorNotification(Strings.InsufficientFunds);
                return;
            }

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
        #endregion
    }
}
