using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

using Neo.Core;
using Neo.Cryptography;
using Neo.Cryptography.ECC;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.IO;
using Neo.Properties;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Neo.UI.Accounts;
using Neo.UI.Assets;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.MVVM;
using Neo.UI.Contracts;
using Neo.UI.Development;
using Neo.UI.Messages;
using Neo.UI.Models;
using Neo.UI.Transactions;
using Neo.UI.Updater;
using Neo.UI.Wallets;
using Neo.UI.Voting;

namespace Neo.UI
{
    public class MainViewModel : ViewModelBase,
        IHandle<UpdateApplicationMessage>,
        IHandle<CreateWalletMessage>,
        IHandle<OpenWalletMessage>
    {
        private static readonly UInt160 RecycleScriptHash = new[] { (byte)OpCode.PUSHT }.ToScriptHash();

        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        private AccountItem selectedAccount;
        private AssetItem selectedAsset;
        private TransactionItem selectedTransaction;

        private bool balanceChanged = false;
        private bool checkNep5Balance = false;
        private DateTime persistenceTime = DateTime.MinValue;

        private bool blockProgressIndeterminate;
        private int blockProgress;


        private string newVersionLabel;
        private bool newVersionVisible;

        private Timer uiUpdateTimer;
        private readonly object uiUpdateTimerLock = new object();

        public MainViewModel()
        {
            this.certificateQueryResultCache = new Dictionary<ECPoint, CertificateQueryResult>();

            this.Accounts = new ObservableCollection<AccountItem>();
            this.Assets = new ObservableCollection<AssetItem>();
            this.Transactions = new ObservableCollection<TransactionItem>();

            EventAggregator.Current.Subscribe(this);

            this.SetupUIUpdateTimer();

            this.StartUIUpdateTimer();
        }

        #region Public Properties

        public ObservableCollection<AccountItem> Accounts { get; }

        public ObservableCollection<AssetItem> Assets { get; }

        public ObservableCollection<TransactionItem> Transactions { get; }

        public bool AccountMenuItemsEnabled => App.CurrentWallet != null;

        public string BlockHeight => $"{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
        public int NodeCount => Program.LocalNode.RemoteNodeCount;

        public AccountItem SelectedAccount
        {
            get => this.selectedAccount;
            set
            {
                if (this.selectedAccount == value) return;

                this.selectedAccount = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.ViewPrivateKeyEnabled));
                NotifyPropertyChanged(nameof(this.ViewContractEnabled));
                NotifyPropertyChanged(nameof(this.ShowVotingDialogEnabled));
                NotifyPropertyChanged(nameof(this.CopyAddressToClipboardEnabled));
                NotifyPropertyChanged(nameof(this.DeleteAccountEnabled));
            }
        }

        public AssetItem SelectedAsset
        {
            get { return this.selectedAsset; }
            set
            {
                if (this.selectedAsset == value) return;

                this.selectedAsset = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.ViewCertificateEnabled));
                NotifyPropertyChanged(nameof(this.DeleteAssetEnabled));
            }
        }

        public TransactionItem SelectedTransaction
        {
            get => this.selectedTransaction;
            set
            {
                if (this.selectedTransaction == value) return;

                this.selectedTransaction = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.CopyTransactionIdEnabled));
            }
        }

        public bool BlockProgressIndeterminate
        {
            get { return this.blockProgressIndeterminate; }
            set
            {
                if (this.blockProgressIndeterminate == value) return;

                this.blockProgressIndeterminate = value;

                NotifyPropertyChanged();
            }
        }

        public int BlockProgress
        {
            get { return this.blockProgress; }
            set
            {
                if (this.blockProgress == value) return;

                this.blockProgress = value;

                NotifyPropertyChanged();
            }
        }

        public bool ViewPrivateKeyEnabled =>
            this.SelectedAccount != null &&
            this.SelectedAccount.Contract != null &&
            this.SelectedAccount.Contract.IsStandard;

        public bool ViewContractEnabled =>
            this.SelectedAccount != null &&
            this.SelectedAccount.Contract != null;

        public bool ShowVotingDialogEnabled =>
            this.SelectedAccount != null &&
            this.SelectedAccount.Contract != null &&
            this.SelectedAccount.Neo > Fixed8.Zero;

        public bool CopyAddressToClipboardEnabled => this.SelectedAccount != null;

        public bool DeleteAccountEnabled => this.SelectedAccount != null;

        public bool ViewCertificateEnabled
        {
            get
            {
                if (this.SelectedAsset == null) return false;

                if (this.SelectedAsset.State == null)
                {
                    return false;
                }
                else
                {
                    var queryResult = GetCertificateQueryResult(this.SelectedAsset.State);

                    if (queryResult == null)
                    {
                        return false;
                    }
                    else
                    {
                        return queryResult.Type == CertificateQueryResultType.Good ||
                            queryResult.Type == CertificateQueryResultType.Expired ||
                                queryResult.Type == CertificateQueryResultType.Invalid;
                    }
                }
            }
        }

        public bool DeleteAssetEnabled => this.SelectedAsset != null &&
            (this.SelectedAsset.State == null ||
                (this.SelectedAsset.State.AssetType != AssetType.GoverningToken &&
                this.SelectedAsset.State.AssetType != AssetType.UtilityToken));

        public bool CopyTransactionIdEnabled => this.SelectedTransaction != null;

        #endregion Public Properies

        #region Tool Strip Menu Commands

        public ICommand CreateWalletCommand => new RelayCommand(CreateWallet);

        public ICommand OpenWalletCommand => new RelayCommand(OpenWallet);

        public ICommand CloseWalletCommand => new RelayCommand(this.CloseWallet);

        public ICommand ChangePasswordCommand => new RelayCommand(ChangePassword);

        public ICommand RebuildIndexCommand => new RelayCommand(this.RebuildIndex);

        public ICommand RestoreAccountsCommand => new RelayCommand(this.RestoreAccounts);

        public ICommand ExitCommand => new RelayCommand(this.Exit);

        public ICommand TransferCommand => new RelayCommand(Transfer);

        public ICommand ShowTransactionDialogCommand => new RelayCommand(ShowTransactionDialog);

        public ICommand ShowSigningDialogCommand => new RelayCommand(ShowSigningDialog);

        public ICommand ClaimCommand => new RelayCommand(Claim);

        public ICommand RequestCertificateCommand => new RelayCommand(RequestCertificate);

        public ICommand AssetRegistrationCommand => new RelayCommand(AssetRegistration);

        public ICommand DistributeAssetCommand => new RelayCommand(DistributeAsset);

        public ICommand DeployContractCommand => new RelayCommand(DeployContract);

        public ICommand InvokeContractCommand => new RelayCommand(InvokeContract);

        public ICommand ShowElectionDialogCommand => new RelayCommand(ShowElectionDialog);

        public ICommand ShowOptionsDialogCommand => new RelayCommand(ShowOptionsDialog);

        public ICommand CheckForHelpCommand => new RelayCommand(CheckForHelp);

        public ICommand ShowOfficialWebsiteCommand => new RelayCommand(ShowOfficialWebsite);

        public ICommand ShowDeveloperToolsCommand => new RelayCommand(ShowDeveloperTools);

        public ICommand AboutNeoCommand => new RelayCommand(ShowAboutNeoDialog);

        public ICommand ShowUpdateDialogCommand => new RelayCommand(ShowUpdateDialog);

        #endregion Tool Strip Menu Commands

        #region Context Menu Commands

        /*
         * Account Context Menu Commands
         */
        public ICommand CreateNewAddressCommand => new RelayCommand(this.CreateNewKey);

        public ICommand ImportWifPrivateKeyCommand => new RelayCommand(this.ImportWifPrivateKey);
        public ICommand ImportFromCertificateCommand => new RelayCommand(this.ImportCertificate);

        public ICommand ImportWatchOnlyAddressCommand => new RelayCommand(this.ImportWatchOnlyAddress);

        public ICommand CreateMultiSignatureContractAddressCommand => new RelayCommand(this.CreateMultiSignatureContract);
        public ICommand CreateLockContractAddressCommand => new RelayCommand(this.CreateLockAddress);

        public ICommand CreateCustomContractAddressCommand => new RelayCommand(this.ImportCustomContract);

        public ICommand ViewPrivateKeyCommand => new RelayCommand(this.ViewPrivateKey);
        public ICommand ViewContractCommand => new RelayCommand(this.ViewContract);
        public ICommand ShowVotingDialogCommand => new RelayCommand(this.ShowVotingDialog);
        public ICommand CopyAddressToClipboardCommand => new RelayCommand(this.CopyAddressToClipboard);
        public ICommand DeleteAccountCommand => new RelayCommand(this.DeleteAccount);


        /*
         * Asset Context Menu Commands
         */
        public ICommand ViewCertificateCommand => new RelayCommand(this.ViewCertificate);
        public ICommand DeleteAssetCommand => new RelayCommand(this.DeleteAsset);


        /*
         * Asset Context Menu Commands
         */
        public ICommand CopyTransactionIdCommand => new RelayCommand(this.CopyTransactionId);

        #endregion Context Menu Commands

        #region New Version Properties

        public string NewVersionLabel
        {
            get => this.newVersionLabel;
            set
            {
                if (this.newVersionLabel == value) return;

                this.newVersionLabel = value;

                NotifyPropertyChanged();
            }
        }

        public bool NewVersionVisible
        {
            get => this.newVersionVisible;
            set
            {
                if (this.newVersionVisible == value) return;

                this.newVersionVisible = value;

                NotifyPropertyChanged();
            }
        }

        #endregion New Version Properties

        public AccountItem GetAccount(string address)
        {
            return this.Accounts.FirstOrDefault(account => account.Address == address);
        }

        public AssetItem GetAsset(UInt256 assetId)
        {
            return this.Assets.FirstOrDefault(asset =>
            {
                if (asset.State == null) return false;

                return asset.State.AssetId.Equals(assetId);
            });
        }

        public int GetTransactionIndex(string transactionId)
        {
            for (int i = 0; i < this.Transactions.Count; i++)
            {
                if (this.Transactions[i].Id == transactionId) return i;
            }

            // Could not find transaction
            return -1;
        }

        #region Wallet Methods

        private void AddAddress(UInt160 scriptHash, bool selected = false)
        {
            var address = Wallet.ToAddress(scriptHash);
            var item = this.GetAccount(address);

            if (item == null)
            {
                //var group = this.Accounts.Groups["watchOnlyGroup"];
                item = new AccountItem
                {
                    Address = address,
                    Neo = Fixed8.Zero,
                    Gas = Fixed8.Zero
                };

                this.Accounts.Add(item);
            }

            this.SelectedAccount = selected ? item : null;
        }

        private void AddContract(VerificationContract contract, bool selected = false)
        {
            var item = this.GetAccount(contract.Address);

            if (item?.ScriptHash != null)
            {
                this.Accounts.Remove(item);
                item = null;
            }

            if (item == null)
            {
                //var group = contract.IsStandard ? this.Accounts.Groups["standardContractGroup"] : this.Accounts.Groups["nonstandardContractGroup"];
                item = new AccountItem
                {
                    Address = contract.Address,
                    Neo = Fixed8.Zero,
                    Gas = Fixed8.Zero,
                    Contract = contract
                };

                this.Accounts.Add(item);
            }

            this.SelectedAccount = selected ? item : null;
        }

        private void Blockchain_PersistCompleted(object sender, Block block)
        {
            this.persistenceTime = DateTime.Now;
            if (App.CurrentWallet != null)
            {
                this.checkNep5Balance = true;

                var coins = App.CurrentWallet.GetCoins();
                if (coins.Any(coin => !coin.State.HasFlag(CoinState.Spent) &&
                    coin.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)))
                {
                    balanceChanged = true;
                }
            }
            CurrentWallet_TransactionsChanged(Enumerable.Empty<TransactionInfo>());
        }

        private void ChangeWallet(UserWallet wallet)
        {
            if (App.CurrentWallet != null)
            {
                // Dispose current wallet
                App.CurrentWallet.BalanceChanged -= CurrentWallet_BalanceChanged;
                App.CurrentWallet.TransactionsChanged -= CurrentWallet_TransactionsChanged;
                App.CurrentWallet.Dispose();
            }

            this.Accounts.Clear();
            this.Assets.Clear();
            this.Transactions.Clear();

            App.CurrentWallet = wallet;

            if (App.CurrentWallet != null)
            {
                // Setup wallet
                var transactions = App.CurrentWallet.LoadTransactions();

                CurrentWallet_TransactionsChanged(transactions);
                App.CurrentWallet.BalanceChanged += CurrentWallet_BalanceChanged;
                App.CurrentWallet.TransactionsChanged += CurrentWallet_TransactionsChanged;
            }

            NotifyPropertyChanged(nameof(this.AccountMenuItemsEnabled));

            if (App.CurrentWallet != null)
            {
                // Load accounts
                foreach (var scriptHash in App.CurrentWallet.GetAddresses())
                {
                    var contract = App.CurrentWallet.GetContract(scriptHash);
                    if (contract == null)
                    {
                        AddAddress(scriptHash);
                    }
                    else
                    {
                        AddContract(contract);
                    }
                }
            }
            balanceChanged = true;
            checkNep5Balance = true;
        }

        private void CurrentWallet_BalanceChanged(object sender, EventArgs e)
        {
            balanceChanged = true;
        }

        private void CurrentWallet_TransactionsChanged(IEnumerable<TransactionInfo> transactions)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action<object, IEnumerable<TransactionInfo>>(CurrentWallet_TransactionsChanged), null, transactions);
        }

        private void CurrentWallet_TransactionsChanged(object sender, IEnumerable<TransactionInfo> transactions)
        {
            // Update transaction list
            foreach (var transactionInfo in transactions)
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
                    this.Transactions[transactionIndex] = transactionItem;
                }
                else
                {
                    // Add transaction to list
                    this.Transactions.Insert(0, transactionItem);
                }
            }

            // Update transaction confirmations
            foreach (var item in this.Transactions)
            {
                uint transactionHeight = 0;

                if (item.Info != null && item.Info.Height != null)
                {
                    transactionHeight = item.Info.Height.Value;
                }

                var confirmations = ((int)Blockchain.Default.Height) - ((int)transactionHeight) + 1;

                item.SetConfirmations(confirmations);
            }
        }

        #endregion Wallet Methods

        #region Blockchain Methods

        private void ImportBlocksIfRequired()
        {
            const string acc_path = "chain.acc";
            const string acc_zip_path = acc_path + ".zip";

            // Check if blocks need importing
            if (File.Exists(acc_path))
            {
                // Import blocks
                using (var fileStream = new FileStream(acc_path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    ImportBlocks(fileStream);
                }
                File.Delete(acc_path);
            }
            else if (File.Exists(acc_zip_path))
            {
                using (var fileStream = new FileStream(acc_zip_path, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                using (var zipStream = zip.GetEntry(acc_path).Open())
                {
                    ImportBlocks(zipStream);
                }
                File.Delete(acc_zip_path);
            }
        }

        private void ImportBlocks(Stream stream)
        {
            var blockchain = (LevelDBBlockchain)Blockchain.Default;
            blockchain.VerifyBlocks = false;
            using (var reader = new BinaryReader(stream))
            {
                uint count = reader.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    var array = reader.ReadBytes(reader.ReadInt32());
                    if (height > Blockchain.Default.Height)
                    {
                        var block = array.AsSerializable<Block>();
                        Blockchain.Default.AddBlock(block);
                    }
                }
            }
            blockchain.VerifyBlocks = true;
        }

        public void Load()
        {
            Task.Run(() =>
            {
                CheckForNewerVersion();

                ImportBlocksIfRequired();

                Blockchain.PersistCompleted += Blockchain_PersistCompleted;

                // Start node
                Program.LocalNode.Start(Settings.Default.NodePort, Settings.Default.WsPort);
            });
        }

        public void Close()
        {
            Blockchain.PersistCompleted -= Blockchain_PersistCompleted;
            this.CloseWallet();
        }

        private void CheckForNewerVersion()
        {
            var latestVersion = VersionHelper.LatestVersion;
            var currentVersion = VersionHelper.CurrentVersion;

            if (latestVersion == null || latestVersion <= currentVersion) return;

            this.NewVersionLabel = $"{Strings.DownloadNewVersion}: {latestVersion}";
            this.NewVersionVisible = true;
        }

        #endregion Blockchain Methods

        #region UI Update Methods

        private void SetupUIUpdateTimer()
        {
            lock (this.uiUpdateTimerLock)
            {
                if (this.uiUpdateTimer != null)
                {
                    // Stop previous timer
                    this.uiUpdateTimer.Stop();

                    this.uiUpdateTimer.Elapsed -= this.UpdateUI;

                    this.uiUpdateTimer.Dispose();

                    this.uiUpdateTimer = null;
                }

                var timer = new Timer
                {
                    Interval = 500,
                    Enabled = true,
                    AutoReset = true
                };

                timer.Elapsed += this.UpdateUI;

                this.uiUpdateTimer = timer;
            }
        }

        private void StartUIUpdateTimer()
        {
            lock (this.uiUpdateTimerLock)
            {
                this.uiUpdateTimer.Start();
            }
        }

        private void StopUIUpdateTimer()
        {
            lock (this.uiUpdateTimerLock)
            {
                this.uiUpdateTimer.Start();
            }
        }

        private void UpdateUI(object sender, System.Timers.ElapsedEventArgs e)
        {
            var persistenceSpan = DateTime.Now - this.persistenceTime;

            this.UpdateBlockProgress(persistenceSpan);

            this.UpdateWallet(persistenceSpan);
        }

        private void UpdateBlockProgress(TimeSpan persistenceSpan)
        {
            NotifyPropertyChanged(nameof(this.BlockHeight));
            NotifyPropertyChanged(nameof(this.NodeCount));

            if (persistenceSpan < TimeSpan.Zero) persistenceSpan = TimeSpan.Zero;

            if (persistenceSpan > Blockchain.TimePerBlock)
            {
                this.BlockProgressIndeterminate = true;
            }
            else
            {
                this.BlockProgressIndeterminate = true;
                this.BlockProgress = persistenceSpan.Seconds;
            }
        }

        private void UpdateWallet(TimeSpan persistenceSpan)
        {
            if (App.CurrentWallet == null) return;

            this.UpdateAssetBalances();

            this.UpdateNEP5TokenBalances(persistenceSpan);
        }

        private void UpdateAssetBalances()
        {
            if (App.CurrentWallet.WalletHeight > Blockchain.Default.Height + 1) return;

            if (balanceChanged)
            {
                var coins = App.CurrentWallet?.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)) ?? Enumerable.Empty<Coin>();
                var bonus_available = Blockchain.CalculateBonus(App.CurrentWallet.GetUnclaimedCoins().Select(p => p.Reference));
                var bonus_unavailable = Blockchain.CalculateBonus(coins.Where(p => p.State.HasFlag(CoinState.Confirmed) && p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).Select(p => p.Reference), Blockchain.Default.Height + 1);
                var bonus = bonus_available + bonus_unavailable;
                var assets = coins.GroupBy(p => p.Output.AssetId, (k, g) => new
                {
                    Asset = Blockchain.Default.GetAssetState(k),
                    Value = g.Sum(p => p.Output.Value),
                    Claim = k.Equals(Blockchain.UtilityToken.Hash) ? bonus : Fixed8.Zero
                }).ToDictionary(p => p.Asset.AssetId);
                if (bonus != Fixed8.Zero && !assets.ContainsKey(Blockchain.UtilityToken.Hash))
                {
                    assets[Blockchain.UtilityToken.Hash] = new
                    {
                        Asset = Blockchain.Default.GetAssetState(Blockchain.UtilityToken.Hash),
                        Value = Fixed8.Zero,
                        Claim = bonus
                    };
                }
                var balanceNeo = coins.Where(p => p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
                var balanceGas = coins.Where(p => p.Output.AssetId.Equals(Blockchain.UtilityToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
                foreach (var account in this.Accounts)
                {
                    var script_hash = Wallet.ToScriptHash(account.Address);
                    var neo = balanceNeo.ContainsKey(script_hash) ? balanceNeo[script_hash] : Fixed8.Zero;
                    var gas = balanceGas.ContainsKey(script_hash) ? balanceGas[script_hash] : Fixed8.Zero;
                    account.Neo = neo;
                    account.Gas = gas;
                }
                foreach (var asset in this.Assets.Where(item => item.State != null))
                {
                    if (!assets.ContainsKey(asset.State.AssetId))
                    {
                        this.Assets.Remove(asset);
                    }
                }
                foreach (var asset in assets.Values)
                {
                    var value_text = asset.Value.ToString() + (asset.Asset.AssetId.Equals(Blockchain.UtilityToken.Hash) ? $"+({asset.Claim})" : "");

                    var item = this.GetAsset(asset.Asset.AssetId);

                    if (item != null)
                    {
                        item.Value = value_text;
                    }
                    else
                    {
                        string asset_name = asset.Asset.AssetType == AssetType.GoverningToken ? "NEO" :
                                            asset.Asset.AssetType == AssetType.UtilityToken ? "NeoGas" :
                                            asset.Asset.GetName();

                        this.Assets.Add(new AssetItem
                        {
                            Name = asset_name,
                            Type = asset.Asset.AssetType.ToString(),
                            Issuer = $"{Strings.UnknownIssuer}[{asset.Asset.Owner}]",
                            Value = value_text
                        });

                        /*this.Assets.Groups["unchecked"]
                        {
                            Name = asset.Asset.AssetId.ToString(),
                            Tag = asset.Asset,
                            UseItemStyleForSubItems = false
                        };*/
                    }
                }
                balanceChanged = false;
            }
            foreach (var item in this.Assets)//.Groups["unchecked"].Items)
            {
                if (item.State == null) continue;

                var asset = item.State;

                var queryResult = GetCertificateQueryResult(asset);

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

        private void UpdateNEP5TokenBalances(TimeSpan persistenceSpan)
        {
            if (!checkNep5Balance) return;

            if (persistenceSpan <= TimeSpan.FromSeconds(2)) return;

            // Update balances
            var addresses = App.CurrentWallet.GetAddresses().ToArray();
            foreach (var s in Settings.Default.NEP5Watched)
            {
                var script_hash = UInt160.Parse(s);
                byte[] script;
                using (var builder = new ScriptBuilder())
                {
                    foreach (var address in addresses)
                    {
                        builder.EmitAppCall(script_hash, "balanceOf", address);
                    }
                    builder.Emit(OpCode.DEPTH, OpCode.PACK);
                    builder.EmitAppCall(script_hash, "decimals");
                    builder.EmitAppCall(script_hash, "name");
                    script = builder.ToArray();
                }
                var engine = ApplicationEngine.Run(script);
                if (engine.State.HasFlag(VMState.FAULT)) continue;
                var name = engine.EvaluationStack.Pop().GetString();
                var decimals = (byte)engine.EvaluationStack.Pop().GetBigInteger();
                var amount = engine.EvaluationStack.Pop().GetArray().Aggregate(BigInteger.Zero, (x, y) => x + y.GetBigInteger());
                if (amount == 0) continue;
                var balance = new BigDecimal(amount, decimals);
                string value_text = balance.ToString();

                var item = (AssetItem)null;//this.GetAsset(script_hash);

                if (item != null)
                {
                    //this.Assets[script_hash.ToString()].Value = value_text;
                }
                else
                {
                    this.Assets.Add(new AssetItem
                    {
                        Name = name,
                        Type = "NEP-5",
                        Issuer = $"ScriptHash:{script_hash}",
                        Value = value_text,
                    });

                    /*this.Assets.Groups["checked"]
                    {
                        Name = script_hash.ToString(),
                        UseItemStyleForSubItems = false
                    };*/
                }
            }
            checkNep5Balance = false;
        }

        #endregion UI Update Methods

        #region Message Handlers

        public void Handle(UpdateApplicationMessage message)
        {
            // Close window
            this.TryClose();

            // Start update
            Process.Start(message.UpdateScriptPath);
        }

        public void Handle(CreateWalletMessage message)
        {
            if (string.IsNullOrEmpty(message.WalletPath) || string.IsNullOrEmpty(message.Password)) return;

            var wallet = UserWallet.Create(message.WalletPath, message.Password);

            ChangeWallet(wallet);
            Settings.Default.LastWalletPath = message.WalletPath;
            Settings.Default.Save();
        }

        public void Handle(OpenWalletMessage message)
        {
            if (UserWallet.GetVersion(message.WalletPath) < Version.Parse("1.3.5"))
            {
                if (MessageBox.Show(Strings.MigrateWalletMessage, Strings.MigrateWalletCaption,
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes)
                        != MessageBoxResult.Yes) return;

                var path_old = Path.ChangeExtension(message.WalletPath, ".old.db3");
                var path_new = Path.ChangeExtension(message.WalletPath, ".new.db3");
                UserWallet.Migrate(message.WalletPath, path_new);
                File.Move(message.WalletPath, path_old);
                File.Move(path_new, message.WalletPath);
                MessageBox.Show($"{Strings.MigrateWalletSucceedMessage}\n{path_old}");
            }
            UserWallet wallet;
            try
            {
                wallet = UserWallet.Open(message.WalletPath, message.Password);
            }
            catch (CryptographicException)
            {
                MessageBox.Show(Strings.PasswordIncorrect);
                return;
            }
            if (message.RepairMode) wallet.Rebuild();
            ChangeWallet(wallet);
            Settings.Default.LastWalletPath = message.WalletPath;
            Settings.Default.Save();
        }

        #endregion Message Handlers

        #region Main Menu Command Methods

        private static void CreateWallet()
        {
            var view = new CreateWalletView();
            view.ShowDialog();
        }

        private static void OpenWallet()
        {
            var view = new OpenWalletView();
            view.ShowDialog();
        }

        public void CloseWallet()
        {
            this.ChangeWallet(null);
        }

        private static void ChangePassword()
        {
            var view = new ChangePasswordView();
            view.ShowDialog();
        }

        private void RebuildIndex()
        {
            this.Assets.Clear();
            this.Transactions.Clear();
            App.CurrentWallet.Rebuild();
        }

        private void RestoreAccounts()
        {
            var view = new RestoreAccountsView();
            view.ShowDialog();

            var contracts = view.GetContracts();

            if (contracts == null) return;

            foreach (var contract in contracts)
            {
                App.CurrentWallet.AddContract(contract);
                AddContract(contract, true);
            }
        }

        private void Exit()
        {
            this.TryClose();
        }

        private static void Transfer()
        {
            var view = new TransferView();
            view.ShowDialog();

            var transaction = view.GetTransaction();

            if (transaction == null) return;

            if (transaction is InvocationTransaction itx)
            {
                var invokeContractView = new InvokeContractView(itx);
                invokeContractView.ShowDialog();

                transaction = invokeContractView.GetTransaction();

                if (transaction == null) return;
            }

            Base.Helper.SignAndShowInformation(transaction);
        }

        private static void ShowTransactionDialog()
        {
            using (var form = new TradeForm())
            {
                form.ShowDialog();
            }
        }

        private static void ShowSigningDialog()
        {
            var view = new SignatureView();
            view.ShowDialog();
        }

        private static void Claim()
        {
            Base.Helper.Show<ClaimView>();
        }

        private static void RequestCertificate()
        {
            var view = new CertificateRequestView();
            view.ShowDialog();
        }

        private static void AssetRegistration()
        {
            var assetRegistrationView = new AssetRegistrationView();
            assetRegistrationView.ShowDialog();

            var transactionResult = assetRegistrationView.GetTransaction();

            if (transactionResult == null) return;


            var invokeContractView = new InvokeContractView(transactionResult);
            invokeContractView.ShowDialog();

            transactionResult = invokeContractView.GetTransaction();

            if (transactionResult == null) return;

            Base.Helper.SignAndShowInformation(transactionResult);
        }

        private static void DistributeAsset()
        {
            using (var dialog = new IssueDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                Base.Helper.SignAndShowInformation(dialog.GetTransaction());
            }
        }

        private static void DeployContract()
        {
            var view = new DeployContractView();
            view.ShowDialog();

            var transactionResult = view.GetTransaction();

            if (transactionResult == null) return;


            var invokeContractView = new InvokeContractView(transactionResult);
            invokeContractView.ShowDialog();

            transactionResult = invokeContractView.GetTransaction();

            if (transactionResult == null) return;


            Base.Helper.SignAndShowInformation(transactionResult);
        }

        private static void InvokeContract()
        {
            var view = new InvokeContractView();
            view.ShowDialog();
        }

        private static void ShowElectionDialog()
        {
            var electionView = new ElectionView();
            electionView.ShowDialog();

            var transactionResult = electionView.GetTransactionResult();

            if (transactionResult == null) return;

            var invokeContractView = new InvokeContractView(transactionResult);
            invokeContractView.ShowDialog();

            transactionResult = invokeContractView.GetTransaction();

            if (transactionResult == null) return;

            Base.Helper.SignAndShowInformation(transactionResult);
        }

        private static void ShowOptionsDialog()
        {
            var view = new OptionsView();
            view.ShowDialog();
        }

        private static void CheckForHelp()
        {
            // TODO Implement
        }

        private static void ShowOfficialWebsite()
        {
            Process.Start("https://neo.org/");
        }

        private static void ShowDeveloperTools()
        {
            Base.Helper.Show<DeveloperToolsView>();
        }

        private static void ShowAboutNeoDialog()
        {
            MessageBox.Show($"{Strings.AboutMessage} {Strings.AboutVersion}{Assembly.GetExecutingAssembly().GetName().Version}", Strings.About);
        }

        #endregion Main Menu Command Methods

        #region Account Menu Command Methods

        private void CreateNewKey()
        {
            this.SelectedAccount = null;
            var key = App.CurrentWallet.CreateKey();
            foreach (var contract in App.CurrentWallet.GetContracts(key.PublicKeyHash))
            {
                AddContract(contract, true);
            }
        }

        private void ImportWifPrivateKey()
        {
            var view = new ImportPrivateKeyView();
            view.ShowDialog();

            var wifStrings = view.WifStrings;

            if (wifStrings == null) return;

            var wifStringList = wifStrings.ToList();

            if (!wifStringList.Any()) return;

            // Import private keys
            this.SelectedAccount = null;

            foreach (var wif in wifStringList)
            {
                KeyPair key;
                try
                {
                    key = App.CurrentWallet.Import(wif);
                }
                catch (FormatException)
                {
                    // Skip WIF line
                    continue;
                }
                foreach (var contract in App.CurrentWallet.GetContracts(key.PublicKeyHash))
                {
                    AddContract(contract, true);
                }
            }
        }

        private void ImportCertificate()
        {
            var view = new SelectCertificateView();
            view.ShowDialog();

            if (view.SelectedCertificate == null) return;

            this.SelectedAccount = null;

            KeyPair key;
            try
            {
                key = App.CurrentWallet.Import(view.SelectedCertificate);
            }
            catch
            {
                MessageBox.Show("Certificate import failed!");
                return;
            }

            foreach (var contract in App.CurrentWallet.GetContracts(key.PublicKeyHash))
            {
                AddContract(contract, true);
            }
        }

        private void ImportWatchOnlyAddress()
        {
            if (!InputBox.Show(out var text, Strings.Address, Strings.ImportWatchOnlyAddress)) return;

            if (string.IsNullOrEmpty(text)) return;

            using (var reader = new StringReader(text))
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
                    App.CurrentWallet.AddWatchOnly(scriptHash);
                    AddAddress(scriptHash, true);
                }
            }
        }

        private void CreateMultiSignatureContract()
        {
            using (var dialog = new CreateMultiSigContractDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                var contract = dialog.GetContract();
                if (contract == null)
                {
                    MessageBox.Show(Strings.AddContractFailedMessage);
                    return;
                }
                App.CurrentWallet.AddContract(contract);
                this.SelectedAccount = null;
                AddContract(contract, true);
            }
        }

        private void CreateLockAddress()
        {
            using (var dialog = new CreateLockAccountDialog())
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                var contract = dialog.GetContract();
                if (contract == null)
                {
                    MessageBox.Show(Strings.AddContractFailedMessage);
                    return;
                }
                App.CurrentWallet.AddContract(contract);
                this.SelectedAccount = null;
                AddContract(contract, true);
            }
        }

        private void ImportCustomContract()
        {
            var view = new ImportCustomContractView();
            view.ShowDialog();

            var contract = view.GetContract();

            if (contract == null) return;

            App.CurrentWallet.AddContract(contract);
            this.SelectedAccount = null;
            AddContract(contract, true);
        }

        private void ViewPrivateKey()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var contract = this.SelectedAccount.Contract;
            var key = App.CurrentWallet.GetKeyByScriptHash(contract.ScriptHash);

            var view = new ViewPrivateKeyView(key, contract.ScriptHash);
            view.ShowDialog();
        }

        private void ViewContract()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var contract = this.SelectedAccount.Contract;

            var view = new ViewContractView(contract);
            view.ShowDialog();
        }

        private void ShowVotingDialog()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var contract = this.SelectedAccount.Contract;

            var view = new VotingView(contract.ScriptHash);
            view.ShowDialog();

            var transaction = view.GetTransaction();

            if (transaction == null) return;

            var invokeContractView = new InvokeContractView(transaction);
            invokeContractView.ShowDialog();

            transaction = invokeContractView.GetTransaction();

            if (transaction == null) return;

            Base.Helper.SignAndShowInformation(transaction);
        }

        private void CopyAddressToClipboard()
        {
            if (this.SelectedAccount == null) return;

            try
            {
                Clipboard.SetText(this.SelectedAccount.Address);
            }
            catch (ExternalException) { }
        }

        private void DeleteAccount()
        {
            if (this.SelectedAccount == null) return;

            var accountToDelete = this.SelectedAccount;

            if (MessageBox.Show(Strings.DeleteAddressConfirmationMessage, Strings.DeleteAddressConfirmationCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            var scriptHash = accountToDelete.ScriptHash != null
                ? accountToDelete.ScriptHash
                : accountToDelete.Contract.ScriptHash;

            App.CurrentWallet.DeleteAddress(scriptHash);
            this.Accounts.Remove(accountToDelete);

            balanceChanged = true;
        }

        #endregion Account Menu Command Methods

        #region Asset Menu Command Methods

        private void ViewCertificate()
        {
            if (this.SelectedAsset == null || this.SelectedAsset.State == null) return;

            var hash = Contract.CreateSignatureRedeemScript(this.SelectedAsset.State.Owner).ToScriptHash();
            var address = Wallet.ToAddress(hash);
            var path = Path.Combine(Settings.Default.CertCachePath, $"{address}.cer");
            Process.Start(path);
        }

        private void DeleteAsset()
        {
            if (this.SelectedAsset == null || this.SelectedAsset.State == null) return;

            var value = App.CurrentWallet.GetAvailable(this.SelectedAsset.State.AssetId);

            if (MessageBox.Show($"{Strings.DeleteAssetConfirmationMessage}\n{string.Join("\n", $"{this.SelectedAsset.State.GetName()}:{value}")}",
                Strings.DeleteConfirmation, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            var transaction = App.CurrentWallet.MakeTransaction(new ContractTransaction
            {
                Outputs = new TransactionOutput[]
                {
                    new TransactionOutput
                    {
                        AssetId = this.SelectedAsset.State.AssetId,
                        Value = value,
                        ScriptHash = RecycleScriptHash
                    }
                }
            }, fee: Fixed8.Zero);

            Base.Helper.SignAndShowInformation(transaction);
        }

        #endregion Asset Menu Command Methods

        #region Transaction Menu Command Methods

        private void CopyTransactionId()
        {
            if (this.SelectedTransaction == null) return;
            Clipboard.SetDataObject(this.SelectedTransaction.Id);
        }

        #endregion Transaction Menu Command Methods

        private static void ShowUpdateDialog()
        {
            var dialog = new UpdateView();

            dialog.ShowDialog();
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
                    result = CertificateQueryService.Query(asset.Owner);

                    if (result == null) return null;

                    // Cache query result
                    this.certificateQueryResultCache.Add(asset.Owner, result);
                }

                result = this.certificateQueryResultCache[asset.Owner];
            }

            return result;
        }
    }
}