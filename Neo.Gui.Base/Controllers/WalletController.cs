using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Timers;
using Neo.Core;
using Neo.Gui.Base.Certificates;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Network;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using ECPoint = Neo.Cryptography.ECC.ECPoint;

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

        private readonly IBlockChainController blockChainController;
        private readonly ICertificateQueryService certificateQueryService;
        private readonly INotificationHelper notificationHelper;
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;

        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        private readonly IList<AccountItem> accounts;
        private readonly IList<AssetItem> assets;
        private readonly IList<TransactionItem> transactions;

        private readonly object walletRefreshLock = new object();

        private bool disposed;

        private Timer refreshTimer;

        private UserWallet currentWallet;

        private bool balanceChanged;
        private bool checkNep5Balance;

        private UInt160[] nep5WatchScriptHashes;

        #endregion

        #region Constructor 

        public WalletController(
            IBlockChainController blockChainController,
            ICertificateQueryService certificateQueryService,
            INotificationHelper notificationHelper,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber)
        {
            this.blockChainController = blockChainController;
            this.certificateQueryService = certificateQueryService;
            this.notificationHelper = notificationHelper;
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
            this.blockChainController.Initialize();

            this.certificateQueryService.Initialize(certificateCachePath);

            // Setup automatic refresh timer
            this.refreshTimer = new Timer
            {
                Interval = 500,
                Enabled = true,
                AutoReset = true
            };

            this.refreshTimer.Elapsed += this.Refresh;
        }

        public bool WalletIsOpen => this.currentWallet != null;

        public uint WalletHeight => !this.WalletIsOpen ? 0 : this.currentWallet.WalletHeight;

        public bool WalletIsSynchronized => this.WalletHeight > this.blockChainController.BlockHeight + 1;


        public bool WalletNeedUpgrade(string walletPath)
        {
            if (UserWallet.GetVersion(walletPath) < Version.Parse(MinimumMigratedWalletVersion))
            {
                return true;
            }

            return false;
        }

        public void UpgradeWallet(string walletPath)
        {
            if (string.IsNullOrEmpty(walletPath)) return;

            var pathOld = Path.ChangeExtension(walletPath, ".old.db3");
            var pathNew = Path.ChangeExtension(walletPath, ".new.db3");
            UserWallet.Migrate(walletPath, pathNew);
            File.Move(walletPath, pathOld);
            File.Move(pathNew, walletPath);

            // TODO [AboimPinto]: this string need to be localized.
            this.notificationHelper.ShowInformationNotification("Wallet migration completed.");
        }

        public void CreateWallet(string walletPath, string password)
        {
            var newWallet = UserWallet.Create(walletPath, password);

            this.SetCurrentWallet(newWallet);
        }

        public void OpenWallet(string walletPath, string password, bool repairMode)
        {
            var userWallet = this.OpenWalletWithPath(walletPath, password);
            if (userWallet == null)
            {
                return;
            }

            if (repairMode)
            {
                userWallet.Rebuild();
            }
            this.SetCurrentWallet(userWallet);
        }

        public void CloseWallet()
        {
            this.SetCurrentWallet(null);
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if (!this.WalletIsOpen) return false;

            return this.currentWallet.ChangePassword(oldPassword, newPassword);
        }

        public void RebuildWalletIndexes()
        {
            this.currentWallet.Rebuild();
        }

        public void CreateNewKey()
        {
            var newKey = this.currentWallet.CreateKey();

            var contractsForKey = this.currentWallet.GetContracts(newKey.PublicKeyHash);
            foreach (var contract in contractsForKey)
            {
                this.AddAccountItemFromContract(contract);
            }
        }

        public bool Sign(ContractParametersContext context)
        {
            return this.currentWallet.Sign(context);
        }

        public void Relay(Transaction transaction, bool saveTransaction = true)
        {
            this.blockChainController.Relay(transaction);

            if (saveTransaction)
            {
                this.currentWallet.SaveTransaction(transaction);
            }
        }

        public void Relay(IInventory inventory)
        {
            this.blockChainController.Relay(inventory);
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

        public KeyPair GetKeyByScriptHash(UInt160 scriptHash)
        {
            return this.currentWallet?.GetKeyByScriptHash(scriptHash);
        }

        public KeyPair GetKey(ECPoint publicKey)
        {
            return this.currentWallet?.GetKey(publicKey);
        }

        public KeyPair GetKey(UInt160 publicKeyHash)
        {
            return this.currentWallet?.GetKey(publicKeyHash);
        }

        public IEnumerable<KeyPair> GetKeys()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<KeyPair>();
            }

            return this.currentWallet.GetKeys();
        }

        public IEnumerable<UInt160> GetAddresses()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<UInt160>();
            }

            return this.currentWallet.GetAddresses();
        }

        public IEnumerable<VerificationContract> GetContracts()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<VerificationContract>();
            }

            return this.currentWallet.GetContracts();
        }

        public IEnumerable<VerificationContract> GetContracts(UInt160 publicKeyHash)
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<VerificationContract>();
            }

            return this.currentWallet.GetContracts(publicKeyHash);
        }

        public IEnumerable<Coin> GetCoins()
        {
            // TODO - ISSUE #37 [AboimPinto]: at this point the return should not be a object from the NEO assemblies but a DTO only know by the application with only the necessary fields.

            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<Coin>();
            }

            return this.currentWallet.GetCoins();
        }

        public IEnumerable<Coin> GetUnclaimedCoins()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<Coin>();
            }

            return this.currentWallet.GetUnclaimedCoins();
        }

        public IEnumerable<Coin> FindUnspentCoins()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<Coin>();
            }

            return this.currentWallet.FindUnspentCoins();
        }

        public UInt160 GetChangeAddress()
        {
            return this.currentWallet?.GetChangeAddress();
        }

        public Transaction GetTransaction(UInt256 hash)
        {
            return this.blockChainController.GetTransaction(hash);
        }

        public Transaction GetTransaction(UInt256 hash, out int height)
        {
            return this.blockChainController.GetTransaction(hash, out height);
        }

        public AccountState GetAccountState(UInt160 scriptHash)
        {
            return this.blockChainController.GetAccountState(scriptHash);
        }

        public ContractState GetContractState(UInt160 scriptHash)
        {
            return this.blockChainController.GetContractState(scriptHash);
        }

        public AssetState GetAssetState(UInt256 assetId)
        {
            return this.blockChainController.GetAssetState(assetId);
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
            return this.blockChainController.CalculateBonus(inputs, ignoreClaimed);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            return this.blockChainController.CalculateBonus(inputs, heightEnd);
        }

        public Fixed8 CalculateUnavailableBonusGas(uint height)
        {
            if (!this.WalletIsOpen) return Fixed8.Zero;

            var unspent = this.FindUnspentCoins().Where(p =>p.Output.AssetId.Equals(this.blockChainController.GoverningToken.Hash)).Select(p => p.Reference);
            
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

        public bool WalletContainsAddress(UInt160 scriptHash)
        {
            return this.WalletIsOpen && this.currentWallet.ContainsAddress(scriptHash);
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

        public VerificationContract GetContract(UInt160 scriptHash)
        {
            // TODO - ISSUE #37 [AboimPinto]: at this point the return should not be a object from the NEO assemblies but a DTO only know by the application with only the necessary fields.

            return this.currentWallet?.GetContract(scriptHash);
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
                    this.currentWallet.AddWatchOnly(scriptHash);
                    this.AddAccountItemFromAddress(scriptHash);
                }
            }
        }

        public void DeleteAccount(AccountItem accountToDelete)
        {
            if (accountToDelete == null)
            {
                throw new ArgumentNullException(nameof(accountToDelete));
            }

            var scriptHash = accountToDelete.ScriptHash != null
                ? accountToDelete.ScriptHash
                : accountToDelete.Contract.ScriptHash;

            this.currentWallet.DeleteAddress(scriptHash);

            this.accounts.Remove(accountToDelete);

            this.SetWalletBalanceChanged();
        }

        public Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            return this.currentWallet?.MakeTransaction(transaction);
        }

        public ContractTransaction MakeTransaction(ContractTransaction transaction, UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            return this.currentWallet?.MakeTransaction(transaction, changeAddress, fee);
        }

        public InvocationTransaction MakeTransaction(InvocationTransaction transaction, UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            return this.currentWallet?.MakeTransaction(transaction, changeAddress, fee);
        }

        public IssueTransaction MakeTransaction(IssueTransaction transaction, UInt160 changeAddress = null,
            Fixed8 fee = default(Fixed8))
        {
            return this.currentWallet?.MakeTransaction(transaction, changeAddress, fee);
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
                        AssetId = this.blockChainController.UtilityToken.Hash,
                        Value = this.CalculateBonus(claims),
                        ScriptHash = this.GetChangeAddress()
                    }
                }
            };
        }

        #endregion

        #region IMessageHandler implementation 

        public void HandleMessage(AddContractsMessage message)
        {
            if (message.Contracts == null || !message.Contracts.Any())
            {
                return;
            }

            foreach (var contract in message.Contracts)
            {
                this.currentWallet.AddContract(contract);
                this.AddAccountItemFromContract(contract);
            }
        }

        public void HandleMessage(AddContractMessage message)
        {
            if (message.Contract == null)
            {
                return;
            }

            this.currentWallet.AddContract(message.Contract);
            this.AddAccountItemFromContract(message.Contract);
        }

        public void HandleMessage(ImportPrivateKeyMessage message)
        {
            if (message.WifStrings == null) return;

            if (!message.WifStrings.Any()) return;

            foreach (var wif in message.WifStrings)
            {
                KeyPair key;
                try
                {
                    key = this.currentWallet.Import(wif);
                }
                catch (FormatException)
                {
                    // Skip WIF line
                    continue;
                }
                foreach (var contract in this.currentWallet.GetContracts(key.PublicKeyHash))
                {
                    this.AddAccountItemFromContract(contract);
                }
            }
        }

        public void HandleMessage(ImportCertificateMessage message)
        {
            if (message.SelectedCertificate == null) return;

            KeyPair key;
            try
            {
                key = this.currentWallet.Import(message.SelectedCertificate);
            }
            catch
            {
                //await DialogCoordinator.Instance.ShowMessageAsync(this, string.Empty, "Certificate import failed!");
                return;
            }

            foreach (var contract in this.currentWallet.GetContracts(key.PublicKeyHash))
            {
                this.AddAccountItemFromContract(contract);
            }
        }

        public void HandleMessage(SignTransactionAndShowInformationMessage message)
        {
            var transaction = message.Transaction;

            if (transaction == null)
            {
                this.notificationHelper.ShowErrorNotification(Strings.InsufficientFunds);
                return;
            }

            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(transaction);
            }
            catch (InvalidOperationException)
            {
                this.notificationHelper.ShowErrorNotification(Strings.UnsynchronizedBlock);
                return;
            }

            this.Sign(context);

            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();

                this.Relay(transaction);

                this.notificationHelper.ShowSuccessNotification($"{Strings.SendTxSucceedMessage} {transaction.Hash}");
            }
            else
            {
                this.notificationHelper.ShowSuccessNotification($"{Strings.IncompletedSignatureMessage} {context}");
            }
        }

        public void HandleMessage(BlockchainPersistCompletedMessage message)
        {
            if (this.WalletIsOpen)
            {
                this.checkNep5Balance = true;

                var coins = this.GetCoins();

                if (coins.Any(
                    coin => !coin.State.HasFlag(CoinState.Spent) &&
                            coin.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)))
                {
                    this.balanceChanged = true;
                }
            }

            this.UpdateTransactions();
        }

        #endregion

        #region Private Methods
        
        private void Refresh(object sender, ElapsedEventArgs e)
        {
            lock (this.walletRefreshLock)
            {
                var blockChainStatus = this.blockChainController.GetStatus();

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

        private void SetCurrentWallet(UserWallet wallet)
        {
            if (this.WalletIsOpen)
            {
                // Dispose current wallet
                this.currentWallet.BalanceChanged -= this.CurrentWalletBalanceChanged;
                this.currentWallet.TransactionsChanged -= this.CurrentWalletTransactionsChanged;
                this.currentWallet.Dispose();
            }

            this.messagePublisher.Publish(new ClearAccountsMessage());
            this.messagePublisher.Publish(new ClearAssetsMessage());
            this.messagePublisher.Publish(new ClearTransactionsMessage());

            this.currentWallet = wallet;

            if (this.WalletIsOpen)
            {
                // Setup wallet
                var transactions = this.currentWallet.LoadTransactions();
                this.UpdateTransactions(transactions);

                this.currentWallet.BalanceChanged += this.CurrentWalletBalanceChanged;
                this.currentWallet.TransactionsChanged += this.CurrentWalletTransactionsChanged;
            }

            this.messagePublisher.Publish(new CurrentWalletHasChangedMessage());
            this.LoadWallet();

            this.balanceChanged = true;
            this.checkNep5Balance = true;
        }

        private void CurrentWalletTransactionsChanged(object sender, IEnumerable<TransactionInfo> transactions)
        {
            this.UpdateTransactions(transactions);
        }

        private void CurrentWalletBalanceChanged(object sender, EventArgs e)
        {
            this.balanceChanged = true;
        }

        private UserWallet OpenWalletWithPath(string walletPath, string password)
        {
            try
            {
                return UserWallet.Open(walletPath, password);

            }
            catch (CryptographicException)
            {
                this.notificationHelper.ShowErrorNotification(Strings.PasswordIncorrect);
            }

            return null;
        }

        private void LoadWallet()
        {
            if (!this.WalletIsOpen) return;

            foreach (var walletAddress in this.GetAddresses())
            {
                var contract = this.GetContract(walletAddress);
                if (contract == null)
                {
                    this.AddAccountItemFromAddress(walletAddress);
                }
                else
                {
                    this.AddAccountItemFromContract(contract);
                }
            }
        }

        private void AddAccountItemFromAddress(UInt160 scriptHash)
        {
            var address = Wallet.ToAddress(scriptHash);

            // Check if account item already exists
            var accountItemForAddress = this.accounts.GetAccountItemForAddress(address);

            if (accountItemForAddress != null) return;

            var newAccountItem = new AccountItem
            {
                Address = address,
                Type = AccountType.WatchOnly,
                Neo = Fixed8.Zero,
                Gas = Fixed8.Zero,
                ScriptHash = scriptHash
            };

            this.AddAccountItem(newAccountItem);
        }

        private void AddAccountItemFromContract(VerificationContract contract)
        {
            // Check if account item already exists
            var accountItemForAddress = this.accounts.GetAccountItemForAddress(contract.Address);

            if (accountItemForAddress != null)
            {
                if (accountItemForAddress.ScriptHash != null) // [AboimPinto] what this logic mean?
                {
                    this.accounts.Remove(accountItemForAddress);
                }
                else
                {
                    return;
                }
            }

            var newAccountItem = new AccountItem
            {
                Address = contract.Address,
                Type = contract.IsStandard ? AccountType.Standard : AccountType.NonStandard,
                Neo = Fixed8.Zero,
                Gas = Fixed8.Zero,
                Contract = contract
            };

            this.AddAccountItem(newAccountItem);
        }

        private void AddAccountItem(AccountItem item)
        {
            if (this.accounts.Contains(item)) return;

            this.accounts.Add(item);
            this.messagePublisher.Publish(new AccountAddedMessage(item));
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
                var scriptHash = Wallet.ToScriptHash(account.Address);
                var neo = balanceNeo.ContainsKey(scriptHash) ? balanceNeo[scriptHash] : Fixed8.Zero;
                var gas = balanceGas.ContainsKey(scriptHash) ? balanceGas[scriptHash] : Fixed8.Zero;
                account.Neo = neo;
                account.Gas = gas;
            }
        }

        private void UpdateFirstClassAssetBalances()
        {
            if (this.WalletIsSynchronized) return;
            
            if (balanceChanged)
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

                this.SetWalletBalanceChanged();
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
            var addresses = this.GetAddresses().ToList();

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

        private void UpdateTransactions(IEnumerable<TransactionInfo> transactionInfos = null)
        {
            if (transactionInfos != null)
            {
                // Update transaction list
                foreach (var transactionInfo in transactionInfos)
                {
                    var transactionItem = new TransactionItem
                    {
                        Info = transactionInfo
                    };

                    var transactionIndex = this.GetTransactionIndex(transactionItem.Id);

                    // Check transaction exists in list
                    if (transactionIndex >= 0)
                    {
                        // Update transaction info
                        this.transactions[transactionIndex] = transactionItem;
                    }
                    else
                    {
                        // Add transaction to list
                        this.transactions.Insert(0, transactionItem);
                    }
                }
            }

            // Update transaction confirmations
            foreach (var transactionItem in this.transactions)
            {
                uint transactionHeight = 0;

                if (transactionItem.Info?.Height != null)
                {
                    transactionHeight = transactionItem.Info.Height.Value;
                }

                var confirmations = this.blockChainController.BlockHeight - transactionHeight + 1;

                transactionItem.SetConfirmations((int) confirmations);
            }

            this.messagePublisher.Publish(new TransactionsHaveChangedMessage(this.transactions));
        }

        private void SetWalletBalanceChanged()
        {
            this.balanceChanged = true;
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

        private int GetTransactionIndex(string transactionId)
        {
            for (int i = 0; i < this.transactions.Count; i++)
            {
                if (this.transactions[i].Id == transactionId) return i;
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
                    this.refreshTimer.Stop();
                    this.refreshTimer = null;

                    // Dispose of blockchain controller
                    this.blockChainController.Dispose();

                    this.disposed = true;
                }
            }
        }

        #endregion
    }
}
