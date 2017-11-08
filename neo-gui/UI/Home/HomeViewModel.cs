using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Neo.Core;
using Neo.Cryptography;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.IO;
using Neo.Properties;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;
using Neo.UI.Assets;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.Helpers;
using Neo.UI.Base.MVVM;
using Neo.UI.Contracts;
using Neo.UI.Development;
using Neo.UI.Messages;
using Neo.UI.Options;
using Neo.UI.Transactions;
using Neo.UI.Updater;
using Neo.UI.Wallets;
using Neo.UI.Voting;
using Neo.UI.Base.Messages;

namespace Neo.UI.Home
{
    public class HomeViewModel :
        ViewModelBase,
        ILoadable,
        IMessageHandler<UpdateApplicationMessage>,
        IMessageHandler<WalletBalanceChangedMessage>
    {
        #region Private Fields 
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IDispatcher dispatcher;

        private readonly object uiUpdateTimerLock = new object();

        private bool balanceChanged = false;

        private bool checkNep5Balance = false;
        private DateTime persistenceTime = DateTime.MinValue;

        private bool blockProgressIndeterminate;
        private int blockProgress;


        private string newVersionLabel;
        private bool newVersionVisible;

        private Timer uiUpdateTimer;
        #endregion

        #region Constructor 
        public HomeViewModel(
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber, 
            IDispatcher dispatcher,
            TransactionsViewModel transactionsViewModel)
        {
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;
            this.dispatcher = dispatcher;

            this.TransactionsViewModel = transactionsViewModel;

            this.SetupUIUpdateTimer();

            this.StartUIUpdateTimer();
        }
        #endregion

        #region Public Properties
        public TransactionsViewModel TransactionsViewModel { get; }

        public bool WalletIsOpen => ApplicationContext.Instance.CurrentWallet != null;

        public string BlockHeight => $"{GetWalletHeight()}/{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
        public int NodeCount => Program.LocalNode.RemoteNodeCount;

        public bool BlockProgressIndeterminate
        {
            get => this.blockProgressIndeterminate;
            set
            {
                if (this.blockProgressIndeterminate == value) return;

                this.blockProgressIndeterminate = value;

                NotifyPropertyChanged();
            }
        }

        public int BlockProgress
        {
            get => this.blockProgress;
            set
            {
                if (this.blockProgress == value) return;

                this.blockProgress = value;

                NotifyPropertyChanged();
            }
        }

        #endregion Public Properies

        #region Tool Strip Menu Commands
        public ICommand CreateWalletCommand => new RelayCommand(this.CreateWallet);

        public ICommand OpenWalletCommand => new RelayCommand(this.OpenWallet);

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

        public ICommand ShowSettingsCommand => new RelayCommand(ShowSettings);

        public ICommand CheckForHelpCommand => new RelayCommand(CheckForHelp);

        public ICommand ShowOfficialWebsiteCommand => new RelayCommand(ShowOfficialWebsite);

        public ICommand ShowDeveloperToolsCommand => new RelayCommand(ShowDeveloperTools);

        public ICommand AboutNeoCommand => new RelayCommand(this.ShowAboutNeoDialog);

        public ICommand ShowUpdateDialogCommand => new RelayCommand(ShowUpdateDialog);

        #endregion Tool Strip Menu Commands
        
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

        // TODO Update property to return actual status
        public string BlockStatus => Strings.WaitingForNextBlock + ":";

        #endregion New Version Properties

        #region Wallet Methods

        private void Blockchain_PersistCompleted(object sender, Block block)
        {
            this.persistenceTime = DateTime.UtcNow;
            if (ApplicationContext.Instance.CurrentWallet != null)
            {
                this.checkNep5Balance = true;

                var coins = ApplicationContext.Instance.CurrentWallet.GetCoins();
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
            if (ApplicationContext.Instance.CurrentWallet != null)
            {
                // Dispose current wallet
                ApplicationContext.Instance.CurrentWallet.BalanceChanged -= CurrentWallet_BalanceChanged;
                ApplicationContext.Instance.CurrentWallet.TransactionsChanged -= CurrentWallet_TransactionsChanged;
                ApplicationContext.Instance.CurrentWallet.Dispose();
            }

            this.dispatcher.InvokeOnMainUIThread(() =>
            {
                this.messagePublisher.Publish(new ClearAccountsMessage());
                this.messagePublisher.Publish(new ClearAssetsMessage());

                this.TransactionsViewModel.Transactions.Clear();
            });

            ApplicationContext.Instance.CurrentWallet = wallet;

            if (ApplicationContext.Instance.CurrentWallet != null)
            {
                // Setup wallet
                var transactions = ApplicationContext.Instance.CurrentWallet.LoadTransactions();

                CurrentWallet_TransactionsChanged(transactions);
                ApplicationContext.Instance.CurrentWallet.BalanceChanged += CurrentWallet_BalanceChanged;
                ApplicationContext.Instance.CurrentWallet.TransactionsChanged += CurrentWallet_TransactionsChanged;
            }

            this.messagePublisher.Publish(new EnableMenuItemsMessage());
            NotifyPropertyChanged(nameof(this.WalletIsOpen));

            this.messagePublisher.Publish(new LoadWalletAddressesMessage());

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
            this.TransactionsViewModel.UpdateTransactions(transactions);
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
                var count = reader.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    var array = reader.ReadBytes(reader.ReadInt32());

                    if (height <= Blockchain.Default.Height) continue;

                    var block = array.AsSerializable<Block>();
                    Blockchain.Default.AddBlock(block);
                }
            }
            blockchain.VerifyBlocks = true;
        }

        private void Load()
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

                    this.uiUpdateTimer.Elapsed -= this.UpdateWallet;

                    this.uiUpdateTimer.Dispose();

                    this.uiUpdateTimer = null;
                }

                var timer = new Timer
                {
                    Interval = 500,
                    Enabled = true,
                    AutoReset = true
                };

                timer.Elapsed += this.UpdateWallet;

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
                this.uiUpdateTimer.Stop();
            }
        }

        private void UpdateWallet(object sender, ElapsedEventArgs e)
        {
            var persistenceSpan = DateTime.UtcNow - this.persistenceTime;

            this.UpdateBlockProgress(persistenceSpan);

            this.UpdateBalances(persistenceSpan);
        }

        private void UpdateBlockProgress(TimeSpan persistenceSpan)
        {
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

            NotifyPropertyChanged(nameof(this.BlockHeight));
            NotifyPropertyChanged(nameof(this.NodeCount));
            NotifyPropertyChanged(nameof(this.BlockStatus));
        }

        private void UpdateBalances(TimeSpan persistenceSpan)
        {
            if (ApplicationContext.Instance.CurrentWallet == null) return;

            this.UpdateAssetBalances();

            this.UpdateNEP5TokenBalances(persistenceSpan);
        }

        
        private void UpdateAssetBalances()
        {
            if (ApplicationContext.Instance.CurrentWallet.WalletHeight > Blockchain.Default.Height + 1) return;

            this.messagePublisher.Publish(new UpdateAcountListMessage());
            this.messagePublisher.Publish(new UpdateAssetsBalanceMessage(this.balanceChanged));
        }


        private async void UpdateNEP5TokenBalances(TimeSpan persistenceSpan)
        {
            if (!checkNep5Balance) return;

            if (persistenceSpan <= TimeSpan.FromSeconds(2)) return;

            // Update balances
            var addresses = ApplicationContext.Instance.CurrentWallet.GetAddresses().ToArray();
            foreach (var s in Settings.Default.NEP5Watched)
            {
                var scriptHash = UInt160.Parse(s);
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

                await this.dispatcher.InvokeOnMainUIThread(() =>
                {
                    var item = (AssetItem) null; //this.GetAsset(scriptHash);

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
                        };

                        this.messagePublisher.Publish(new AddAssetMessage(assetItem));
                    }
                });
            }
            checkNep5Balance = false;
        }

        #endregion UI Update Methods
        
        #region Main Menu Command Methods

        private void CreateWallet()
        {
            var view = new CreateWalletView();
            view.ShowDialog();

            if (!view.GetWalletOpenInfo(out var walletPath, out var password)) return;

            if (string.IsNullOrEmpty(walletPath) || string.IsNullOrEmpty(password)) return;

            var wallet = UserWallet.Create(walletPath, password);

            this.ChangeWallet(wallet);
            Settings.Default.LastWalletPath = walletPath;
            Settings.Default.Save();
        }

        private async void OpenWallet()
        {
            var view = new OpenWalletView();
            view.ShowDialog();

            if (!view.GetWalletOpenInfo(out var walletPath, out var password, out var repairMode)) return;
            
            if (UserWallet.GetVersion(walletPath) < Version.Parse("1.3.5"))
            {
                var migrateApproved = await DialogCoordinator.Instance.ShowMessageAsync(this,
                    Strings.MigrateWalletCaption, Strings.MigrateWalletMessage,
                        MessageDialogStyle.AffirmativeAndNegative);

                if (migrateApproved != MessageDialogResult.Affirmative) return;

                var pathOld = Path.ChangeExtension(walletPath, ".old.db3");
                var pathNew = Path.ChangeExtension(walletPath, ".new.db3");
                UserWallet.Migrate(walletPath, pathNew);
                File.Move(walletPath, pathOld);
                File.Move(pathNew, walletPath);

                await DialogCoordinator.Instance.ShowMessageAsync(this, string.Empty, $"{Strings.MigrateWalletSucceedMessage}\n{pathOld}");
            }
            UserWallet wallet;
            try
            {
                wallet = UserWallet.Open(walletPath, password);
            }
            catch (CryptographicException)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(this, string.Empty, Strings.PasswordIncorrect);
                return;
            }
            if (repairMode) wallet.Rebuild();
            ChangeWallet(wallet);
            Settings.Default.LastWalletPath = walletPath;
            Settings.Default.Save();
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

        private async void RebuildIndex()
        {
            await this.dispatcher.InvokeOnMainUIThread(() =>
            {
                this.messagePublisher.Publish(new ClearAssetsMessage());

                this.TransactionsViewModel.Transactions.Clear();
            });

            ApplicationContext.Instance.CurrentWallet.Rebuild();
        }

        private void RestoreAccounts()
        {
            var view = new RestoreAccountsView();
            view.ShowDialog();

            var contracts = view.GetContracts();

            this.messagePublisher.Publish(new RestoreContractsMessage(contracts));
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

            TransactionHelper.SignAndShowInformation(transaction);
        }

        private static void ShowTransactionDialog()
        {
            var view = new TradeView();
            view.ShowDialog();
        }

        private static void ShowSigningDialog()
        {
            var view = new SigningView();
            view.ShowDialog();
        }

        private static void Claim()
        {
            WindowHelper.Show<ClaimView>();
        }

        private static void RequestCertificate()
        {
            var view = new CertificateApplicationView();
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

            TransactionHelper.SignAndShowInformation(transactionResult);
        }

        private static void DistributeAsset()
        {
            var view = new AssetDistributionView();
            view.ShowDialog();

            var transaction = view.GetTransaction();

            if (transaction == null) return;

            TransactionHelper.SignAndShowInformation(transaction);
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


            TransactionHelper.SignAndShowInformation(transactionResult);
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

            TransactionHelper.SignAndShowInformation(transactionResult);
        }

        private static void ShowSettings()
        {
            var view = new SettingsView();
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
            WindowHelper.Show<DeveloperToolsView>();
        }

        private void ShowAboutNeoDialog()
        {
            DialogCoordinator.Instance.ShowMessageAsync(this, Strings.About, $"{Strings.AboutMessage} {Strings.AboutVersion} {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        #endregion Main Menu Command Methods

        private static void ShowUpdateDialog()
        {
            var dialog = new UpdateView();

            dialog.ShowDialog();
        }

        private static uint GetWalletHeight()
        {
            uint walletHeight = 0;

            if (ApplicationContext.Instance.CurrentWallet != null &&
                ApplicationContext.Instance.CurrentWallet.WalletHeight > 0)
            {
                // Set wallet height
                walletHeight = ApplicationContext.Instance.CurrentWallet.WalletHeight - 1;
            }

            return walletHeight;
        }

        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);

            this.Load();
        }

        #region IMessageHandler implementation 
        public void HandleMessage(UpdateApplicationMessage message)
        {
            // Close window
            this.TryClose();

            // Start update
            Process.Start(message.UpdateScriptPath);
        }

        public void HandleMessage(WalletBalanceChangedMessage message)
        {
            this.balanceChanged = message.BalanceChanged;
        }
        #endregion
    }
}