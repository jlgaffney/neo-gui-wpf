using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Assets;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.Dispatching;
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
using Neo.Controllers;

namespace Neo.UI.Home
{
    public class HomeViewModel :
        ViewModelBase,
        ILoadable,
        IMessageHandler<UpdateApplicationMessage>,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<WalletBalanceChangedMessage>,
        IMessageHandler<SignTransactionAndShowInformationMessage>,
        IMessageHandler<BlockProgressMessage>
    {
        #region Private Fields 
        private readonly IBlockChainController blockChainController;
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IDispatcher dispatcher;

        private bool balanceChanged = false;

        private bool blockProgressIndeterminate;
        private int blockProgress;

        private string newVersionLabel;
        private bool newVersionVisible;

        private string blockHeight;
        private int nodeCount;
        private string blockStatus;
        #endregion

        #region Constructor 
        public HomeViewModel(
            IBlockChainController blockChainController,
            IWalletController walletController, 
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber, 
            IDispatcher dispatcher)
        {
            this.blockChainController = blockChainController;
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;
            this.dispatcher = dispatcher;
        }
        #endregion

        #region Public Properties
        public bool WalletIsOpen => this.walletController.WalletIsOpen;

        public string BlockHeight
        {
            get
            {
                return this.blockHeight;
            }
            set
            {
                this.blockHeight = value;
                this.NotifyPropertyChanged(nameof(this.BlockHeight));
            }
        }

        public int NodeCount
        {
            get
            {
                return this.nodeCount;
            }
            set
            {
                this.nodeCount = value;
                this.NotifyPropertyChanged(nameof(this.NodeCount));
            }
        }

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

        public ICommand RestoreAccountsCommand => new RelayCommand(RestoreAccounts);

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
        public string BlockStatus
        {
            get
            {
                return this.blockStatus;
            }
            set
            {
                this.blockStatus = value;
                this.NotifyPropertyChanged(nameof(this.BlockStatus));
            }
        }

        #endregion New Version Properties

        #region Main Menu Command Methods

        private void CreateWallet()
        {
            var view = new CreateWalletView();
            view.ShowDialog();

            if (!view.GetWalletOpenInfo(out var walletPath, out var password)) return;

            if (string.IsNullOrEmpty(walletPath) || string.IsNullOrEmpty(password)) return;

            this.walletController.CreateWallet(walletPath, password);
        }

        private void OpenWallet()
        {
            var view = new OpenWalletView();
            view.ShowDialog();

            //var openWalletDialogResult = this.dialogHelper.ShowDialog<OpenWalletDialogResult>("OpenWalletDialog");
            if (!view.GetWalletOpenInfo(out var walletPath, out var password, out var repairMode)) return;

            this.walletController.OpenWallet(walletPath, password, repairMode);
        }

        public void CloseWallet()
        {
            this.walletController.CloseWallet();
            //this.ChangeWallet(null);
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
                this.messagePublisher.Publish(new ClearTransactionsMessage());
            });

            this.walletController.RebuildWalletIndexes();
        }

        private static void RestoreAccounts()
        {
            var view = new RestoreAccountsView();
            view.ShowDialog();
        }

        private void Exit()
        {
            this.TryClose();
        }

        private static void Transfer()
        {
            var view = new TransferView();
            view.ShowDialog();
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
            var view = new ClaimView();
            view.Show();
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
        }

        private static void DistributeAsset()
        {
            var view = new AssetDistributionView();
            view.ShowDialog();
        }

        private static void DeployContract()
        {
            var view = new DeployContractView();
            view.ShowDialog();
        }

        private static void ShowElectionDialog()
        {
            var electionView = new ElectionView();
            electionView.ShowDialog();
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
            // TODO Issue #40: this static call need to be abstract
            Process.Start("https://neo.org/");
        }

        private static void ShowDeveloperTools()
        {
            var view = new DeveloperToolsView();
            view.Show();
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

        #region ILoadable Implementation 
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion

        private void InvokeContract()
        {
            this.messagePublisher.Publish(new InvokeContractMessage(null));
        }

        #region IMessageHandler implementation 
        public void HandleMessage(UpdateApplicationMessage message)
        {
            // Close window
            this.TryClose();

            // Start update
            Process.Start(message.UpdateScriptPath);
        }

        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            this.NotifyPropertyChanged(nameof(this.WalletIsOpen));
        }

        public void HandleMessage(WalletBalanceChangedMessage message)
        {
            this.balanceChanged = message.BalanceChanged;
        }

        // TODO [AboimPinto] #38: HomeViewModel doesn isn't the message receiver of this message. I could not find any class that is the receiver of this message. This need to be reviewed.
        public void HandleMessage(InvokeContractMessage message)
        {
            var invokeContractView = new InvokeContractView(message.Transaction);
            invokeContractView.ShowDialog();
        }

        // TODO: Issue: #39 - Move these message handlers to a more appropriate place, they don't need to be in HomeViewModel
        public void HandleMessage(SignTransactionAndShowInformationMessage message)
        {
            var transaction = message.Transaction;

            if (transaction == null)
            {
                MessageBox.Show(Strings.InsufficientFunds);
                return;
            }

            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(transaction);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(Strings.UnsynchronizedBlock);
                return;
            }

            this.walletController.Sign(context);

            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();

                // TODO [AboimPinto] this method should be added to the WalletController or the BlockChainController
                this.walletController.SaveTransaction(transaction);
                this.blockChainController.Relay(transaction);

                InformationBox.Show(transaction.Hash.ToString(), Strings.SendTxSucceedMessage, Strings.SendTxSucceedTitle);
            }
            else
            {
                InformationBox.Show(context.ToString(), Strings.IncompletedSignatureMessage, Strings.IncompletedSignatureTitle);
            }
        }

        public void HandleMessage(BlockProgressMessage message)
        {
            this.BlockProgressIndeterminate = message.BlockProgressIndeterminate;
            this.BlockProgress = message.BlockProgress;
            this.BlockHeight = message.BlockHeight;
            this.NodeCount = message.NodeCount;
            this.BlockStatus = message.BlockStatus;
        }
        #endregion
    }
}