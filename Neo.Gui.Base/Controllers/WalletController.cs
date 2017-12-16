using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Timers;

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
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Services;

using CryptographicException = System.Security.Cryptography.CryptographicException;
using DeprecatedWallet = Neo.Implementations.Wallets.EntityFramework.UserWallet;

namespace Neo.Gui.Base.Controllers
{
    public class WalletController :
        IWalletController,
        IMessageHandler<AddContractsMessage>,
        IMessageHandler<AddContractMessage>,
        IMessageHandler<ImportPrivateKeyMessage>,
        IMessageHandler<ImportCertificateMessage>,
        IMessageHandler<SignTransactionAndShowInformationMessage>,
        IMessageHandler<BlockchainPersistCompletedMessage>
    {
        private const string MinimumMigratedWalletVersion = "1.3.5";

        #region Private Fields 

        private readonly IBlockchainController blockchainController;
        private readonly ICertificateQueryService certificateQueryService;
        private readonly INotificationService notificationService;
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;

        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        private readonly IList<AccountItem> accounts;
        private readonly IList<AssetItem> assets;
        private readonly IList<TransactionItem> transactions;

        private readonly object walletRefreshLock = new object();

        private bool initialized;
        private bool disposed;

        private Timer refreshTimer;

        private Wallet currentWallet;

        private bool balanceChanged;
        private bool checkNep5Balance;

        private UInt160[] nep5WatchScriptHashes;

        #endregion

        #region Constructor 

        public WalletController(
            IBlockchainController blockchainController,
            ICertificateQueryService certificateQueryService,
            INotificationService notificationService,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber)
        {
            this.blockchainController = blockchainController;
            this.certificateQueryService = certificateQueryService;
            this.notificationService = notificationService;
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;

            this.messageSubscriber.Subscribe(this);

            this.accounts = new List<AccountItem>();
            this.assets = new List<AssetItem>();
            this.transactions = new List<TransactionItem>();

            this.certificateQueryResultCache = new Dictionary<ECPoint, CertificateQueryResult>();
        }

        #endregion

        #region IWalletController implementation 

        public void Initialize(string certificateCachePath)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IWalletController));
            }

            if (this.initialized)
            {
                throw new Exception(nameof(IWalletController) + " has already been initialized!");
            }

            this.blockchainController.Initialize();

            this.certificateQueryService.Initialize(certificateCachePath);

            // Setup automatic refresh timer
            this.refreshTimer = new Timer
            {
                Interval = 500,
                Enabled = true,
                AutoReset = true
            };

            this.refreshTimer.Elapsed += this.Refresh;

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

        public void CreateWallet(string walletPath, string password)
        {
            var newWallet = new NEP6Wallet(walletPath);

            newWallet.Unlock(password);

            this.SetCurrentWallet(newWallet);
        }

        public void OpenWallet(string walletPath, string password)
        {
            Wallet wallet;
            if (Path.GetExtension(walletPath) == ".db3")
            {
                DeprecatedWallet userWallet;
                try
                {
                    userWallet = DeprecatedWallet.Open(walletPath, password);
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
                    nep6Wallet.Unlock(password);
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
            
            this.SetCurrentWallet(wallet);
        }

        public void CloseWallet()
        {
            this.SetCurrentWallet(null);
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
            this.blockchainController.Relay(transaction);

            if (saveTransaction)
            {
                this.currentWallet.ApplyTransaction(transaction);
            }
        }

        public void Relay(IInventory inventory)
        {
            this.blockchainController.Relay(inventory);
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
            return this.nep5WatchScriptHashes ?? Enumerable.Empty<UInt160>();
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

        public bool CanViewCertificate(AssetItem item)
        {
            if (item.State == null) return false;

            var queryResult = GetCertificateQueryResult(item.State);

            if (queryResult == null) return false;

            return queryResult.Type == CertificateQueryResultType.Good ||
                   queryResult.Type == CertificateQueryResultType.Expired ||
                   queryResult.Type == CertificateQueryResultType.Invalid;
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

        public bool DeleteAccount(AccountItem accountToDelete)
        {
            this.ThrowIfWalletIsNotOpen();

            if (accountToDelete == null)
            {
                throw new ArgumentNullException(nameof(accountToDelete));
            }

            var deletedSuccessfully = this.currentWallet.DeleteAccount(accountToDelete.Account.ScriptHash);

            if (!deletedSuccessfully) return false;

            this.accounts.Remove(accountToDelete);

            this.SetWalletBalanceChangedFlag();

            return true;
        }

        public Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction);
        }

        public ContractTransaction MakeTransaction(ContractTransaction transaction, UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction, changeAddress, fee);
        }

        public InvocationTransaction MakeTransaction(InvocationTransaction transaction, UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.MakeTransaction(transaction, changeAddress, fee);
        }

        public IssueTransaction MakeTransaction(IssueTransaction transaction, UInt160 changeAddress = null,
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

        public UInt160 ToScriptHash(string address)
        {
            return Wallet.ToScriptHash(address);
        }

        public string ToAddress(UInt160 scriptHash)
        {
            return Wallet.ToAddress(scriptHash);
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
            var transaction = message.Transaction;

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

        public void HandleMessage(BlockchainPersistCompletedMessage message)
        {
            if (!this.WalletIsOpen) return;

            this.checkNep5Balance = true;

            var coins = this.GetCoins();

            if (coins.Any(coin => !coin.State.HasFlag(CoinState.Spent) &&
                coin.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)))
            {
                this.SetWalletBalanceChangedFlag();
            }

            this.RefreshTransactionConfirmations();
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
        
        private void Refresh(object sender, ElapsedEventArgs e)
        {
            lock (this.walletRefreshLock)
            {
                var blockChainStatus = this.blockchainController.GetStatus();

                var walletStatus = new WalletStatus(this.WalletHeight, blockChainStatus.Height,
                    blockChainStatus.HeaderHeight,
                    blockChainStatus.NextBlockProgressIsIndeterminate, blockChainStatus.NextBlockProgressFraction,
                    blockChainStatus.NodeCount);

                this.messagePublisher.Publish(new WalletStatusMessage(walletStatus));

                // Update wallet
                if (!this.WalletIsOpen) return;

                this.UpdateAccountBalances();

                this.UpdateFirstClassAssetBalances();

                this.UpdateNEP5TokenBalances(blockChainStatus.TimeSinceLastBlock);
            }
        }

        private void SetCurrentWallet(Wallet wallet)
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

            this.accounts.Clear();
            this.transactions.Clear();
            this.messagePublisher.Publish(new ClearAccountsMessage());
            this.messagePublisher.Publish(new ClearAssetsMessage());
            this.messagePublisher.Publish(new ClearTransactionsMessage());

            this.currentWallet = wallet;

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
                    Transaction = Blockchain.Default.GetTransaction(p, out int height),
                    Height = (uint) height
                }).Where(p => p.Transaction != null).Select(p => new

                {
                    p.Transaction,
                    p.Height,
                    Time = Blockchain.Default.GetHeader(p.Height).Timestamp
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
            
            this.AddTransaction(e.Transaction, transactionHeight, e.Time);

            this.SetWalletBalanceChangedFlag();
        }

        private void AddAccountItem(WalletAccount account)
        {
            // Check if account item already exists
            var accountItemForAddress = this.accounts.GetAccountItemForAddress(account.Address);

            if (accountItemForAddress != null) return;

            var newAccountItem = new AccountItem
            {
                Neo = Fixed8.Zero,
                Gas = Fixed8.Zero,
                Account = account
            };

            if (this.accounts.Contains(newAccountItem)) return;

            this.accounts.Add(newAccountItem);
            this.messagePublisher.Publish(new AccountAddedMessage(newAccountItem));
        }

        private void UpdateAccountBalances()
        {
            var coins = this.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();

            if (!coins.Any()) return;

            var balanceNeo = coins.Where(p => p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
            var balanceGas = coins.Where(p => p.Output.AssetId.Equals(Blockchain.UtilityToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));

            var accountsList = this.accounts.ToList();

            foreach (var account in accountsList)
            {
                var scriptHash = account.Account.ScriptHash;
                var neo = balanceNeo.ContainsKey(scriptHash) ? balanceNeo[scriptHash] : Fixed8.Zero;
                var gas = balanceGas.ContainsKey(scriptHash) ? balanceGas[scriptHash] : Fixed8.Zero;
                account.Neo = neo;
                account.Gas = gas;
            }
        }

        private void UpdateFirstClassAssetBalances()
        {
            if (this.WalletIsSynchronized) return;
            
            if (this.GetWalletBalanceChangedFlag())
            {
                var coins = this.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();
                var bonusAvailable = Blockchain.CalculateBonus(this.GetUnclaimedCoins().Select(p => p.Reference));
                var bonusUnavailable = Blockchain.CalculateBonus(coins.Where(p => p.State.HasFlag(CoinState.Confirmed) && p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).Select(p => p.Reference), Blockchain.Default.Height + 1);
                var bonus = bonusAvailable + bonusUnavailable;

                var assetDictionary = coins.GroupBy(p => p.Output.AssetId, (k, g) => new
                {
                    Asset = Blockchain.Default.GetAssetState(k),
                    Value = g.Sum(p => p.Output.Value),
                    Claim = k.Equals(Blockchain.UtilityToken.Hash) ? bonus : Fixed8.Zero
                }).ToDictionary(p => p.Asset.AssetId);

                if (bonus != Fixed8.Zero && !assetDictionary.ContainsKey(Blockchain.UtilityToken.Hash))
                {
                    assetDictionary[Blockchain.UtilityToken.Hash] = new
                    {
                        Asset = Blockchain.Default.GetAssetState(Blockchain.UtilityToken.Hash),
                        Value = Fixed8.Zero,
                        Claim = bonus
                    };
                }

                foreach (var asset in this.assets.Where(item => item.State != null))
                {
                    if (assetDictionary.ContainsKey(asset.State.AssetId)) continue;

                    this.assets.Remove(asset);
                }

                foreach (var asset in assetDictionary.Values)
                {
                    if (asset.Asset == null || asset.Asset.AssetId == null) continue;

                    var valueText = asset.Value + (asset.Asset.AssetId.Equals(Blockchain.UtilityToken.Hash) ? $"+({asset.Claim})" : "");

                    var item = this.GetAsset(asset.Asset.AssetId);

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

                        var assetItem = new AssetItem
                        {
                            Name = assetName,
                            Type = asset.Asset.AssetType.ToString(),
                            Issuer = $"{Strings.UnknownIssuer}[{asset.Asset.Owner}]",
                            Value = valueText,
                            State = asset.Asset
                        };

                        /*this.assets.Groups["unchecked"]
                        {
                            Name = asset.Asset.AssetId.ToString(),
                            Tag = asset.Asset,
                            UseItemStyleForSubItems = false
                        };*/

                        this.assets.Add(assetItem);
                        this.messagePublisher.Publish(new AssetAddedMessage(assetItem));
                    }
                }

                this.ClearWalletBalanceChangedFlag();
            }


            foreach (var item in this.assets)//.Groups["unchecked"].Items)
            {
                if (item.State == null) continue;

                var asset = item.State;

                var queryResult = this.GetCertificateQueryResult(asset);

                if (queryResult == null) continue;

                using (queryResult)
                {
                    switch (queryResult.Type)
                    {
                        case CertificateQueryResultType.Querying:
                        case CertificateQueryResultType.QueryFailed:
                            break;
                        case CertificateQueryResultType.System:
                            //subitem.ForeColor = Color.Green;
                            item.Issuer = Strings.SystemIssuer;
                            break;
                        case CertificateQueryResultType.Invalid:
                            //subitem.ForeColor = Color.Red;
                            item.Issuer = $"[{Strings.InvalidCertificate}][{asset.Owner}]";
                            break;
                        case CertificateQueryResultType.Expired:
                            //subitem.ForeColor = Color.Yellow;
                            item.Issuer = $"[{Strings.ExpiredCertificate}]{queryResult.Certificate.Subject}[{asset.Owner}]";
                            break;
                        case CertificateQueryResultType.Good:
                            //subitem.ForeColor = Color.Black;
                            item.Issuer = $"{queryResult.Certificate.Subject}[{asset.Owner}]";
                            break;
                    }
                    switch (queryResult.Type)
                    {
                        case CertificateQueryResultType.System:
                        case CertificateQueryResultType.Missing:
                        case CertificateQueryResultType.Invalid:
                        case CertificateQueryResultType.Expired:
                        case CertificateQueryResultType.Good:
                            //item.Group = listView2.Groups["checked"];
                            break;
                    }
                }
            }
        }

        private void UpdateNEP5TokenBalances(TimeSpan timeSinceLastBlock)
        {
            if (!checkNep5Balance) return;

            if (timeSinceLastBlock <= TimeSpan.FromSeconds(2)) return;

            // Update balances
            var addresses = this.GetAccounts().Select(p => p.ScriptHash).ToList();

            foreach (var scriptHash in this.nep5WatchScriptHashes)
            {
                byte[] script;
                using (var builder = new ScriptBuilder())
                {
                    foreach (var address in addresses)
                    {
                        builder.EmitAppCall(scriptHash, "balanceOf", address);
                    }
                    builder.Emit(OpCode.DEPTH, OpCode.PACK);
                    builder.EmitAppCall(scriptHash, "decimals");
                    builder.EmitAppCall(scriptHash, "name");
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

                var item = this.GetAsset(scriptHash); 

                if (item != null)
                {
                    item.Value = valueText;
                }
                else
                {
                    var assetItem = new AssetItem
                    {
                        Name = name,
                        Type = "NEP-5",
                        Issuer = $"ScriptHash:{scriptHash}",
                        Value = valueText,
                        ScriptHashNEP5 = scriptHash
                    };

                    this.assets.Add(assetItem);
                    this.messagePublisher.Publish(new AssetAddedMessage(assetItem));
                }
            }
            checkNep5Balance = false;
        }

        private void AddTransaction(Transaction transaction, uint height, uint timestamp)
        {
            var transactionItem = new TransactionItem(transaction, height, UnixTimeStampToDateTime(timestamp));

            // Add transaction to beginning of list
            this.transactions.Insert(0, transactionItem);
            
            this.messagePublisher.Publish(new TransactionsHaveChangedMessage(this.transactions));
        }

        private DateTime UnixTimeStampToDateTime(uint timeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            dateTime = dateTime.AddSeconds(timeStamp).ToLocalTime();

            return dateTime;
        }

        private void RefreshTransactionConfirmations()
        {
            // Update transaction confirmations
            foreach (var transactionItem in this.transactions)
            {
                var confirmations = this.blockchainController.BlockHeight - transactionItem.Height + 1;

                transactionItem.SetConfirmations((int)confirmations);
            }

            this.messagePublisher.Publish(new TransactionsHaveChangedMessage(this.transactions));
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

        private AssetItem GetAsset(UInt160 scriptHash)
        {
            if (scriptHash == null) return null;

            return this.assets.FirstOrDefault(a => a.ScriptHashNEP5 != null && a.ScriptHashNEP5.Equals(scriptHash));
        }

        private AssetItem GetAsset(UInt256 assetId)
        {
            if (assetId == null) return null;

            return this.assets.FirstOrDefault(a => a.State != null && a.State.AssetId != null && a.State.AssetId.Equals(assetId));
        }

        private int GetTransactionIndex(TransactionItem transactionItem)
        {
            for (int i = 0; i < this.transactions.Count; i++)
            {
                if (this.transactions[i].Equals(transactionItem)) return i;
            }

            // Could not find transaction
            return -1;
        }

        private CertificateQueryResult GetCertificateQueryResult(AssetState asset)
        {
            CertificateQueryResult result;
            if (asset.AssetType == AssetType.GoverningToken || asset.AssetType == AssetType.UtilityToken)
            {
                result = new CertificateQueryResult { Type = CertificateQueryResultType.System };
            }
            else
            {
                if (!this.certificateQueryResultCache.ContainsKey(asset.Owner))
                {
                    result = this.certificateQueryService.Query(asset.Owner);

                    if (result == null) return null;

                    // Cache query result
                    this.certificateQueryResultCache.Add(asset.Owner, result);
                }

                result = this.certificateQueryResultCache[asset.Owner];
            }

            return result;
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

                    // Dispose of blockchain controller
                    this.blockchainController.Dispose();

                    this.disposed = true;
                }
            }
        }

        #endregion
    }
}
