using System;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Assets;
using Neo.Gui.Base.Dialogs.Results.Contracts;
using Neo.Gui.Base.Dialogs.Results.Settings;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Dialogs.Results.Development;
using Neo.Gui.Base.Dialogs.Results.Home;
using Neo.Gui.Base.Dialogs.Results.Transactions;
using Neo.Gui.Base.Dialogs.Results.Voting;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Base.Helpers;
using Neo.Gui.Base.Managers;

namespace Neo.Gui.ViewModels.Home
{
    public class HomeViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IDialogViewModel<HomeDialogResult>,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<InvokeContractMessage>,
        IMessageHandler<NewVersionAvailableMessage>,
        IMessageHandler<UpdateApplicationMessage>,
        IMessageHandler<WalletStatusMessage>
    {
        #region Private Fields
        private const string OfficialWebsiteUrl = "https://neo.org/";

        private readonly IWalletController walletController;
        private readonly IDialogManager dialogManager;
        private readonly IProcessHelper processHelper;
        private readonly ISettingsManager settingsManager;
        private readonly IMessagePublisher messagePublisher;
        private readonly IMessageSubscriber messageSubscriber;

        private bool nextBlockProgressIsIndeterminate;
        private double nextBlockProgressFraction;

        private string newVersionLabel;
        private bool newVersionVisible;

        private string heightStatus;
        private int nodeCount;
        private string blockStatus;
        #endregion
        
        #region Public Properties
        public bool WalletIsOpen => this.walletController.WalletIsOpen;

        public string HeightStatus
        {
            get => this.heightStatus;
            set
            {
                if (this.heightStatus == value) return;

                this.heightStatus = value;

                RaisePropertyChanged();
            }
        }

        public int NodeCount
        {
            get => this.nodeCount;
            set
            {
                if (this.nodeCount == value) return;

                this.nodeCount = value;

                RaisePropertyChanged();
            }
        }

        public bool NextBlockProgressIsIndeterminate
        {
            get => this.nextBlockProgressIsIndeterminate;
            set
            {
                if (this.nextBlockProgressIsIndeterminate == value) return;

                this.nextBlockProgressIsIndeterminate = value;

                RaisePropertyChanged();
            }
        }

        public double NextBlockProgressFraction
        {
            get => this.nextBlockProgressFraction;
            set
            {
                if (this.nextBlockProgressFraction.Equals(value)) return;

                this.nextBlockProgressFraction = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.NextBlockProgressPercentage));
            }
        }

        public int NextBlockProgressPercentage => (int)Math.Round(this.NextBlockProgressFraction * 100.0);

        public string NewVersionLabel
        {
            get => this.newVersionLabel;
            set
            {
                if (this.newVersionLabel == value) return;

                this.newVersionLabel = value;

                RaisePropertyChanged();
            }
        }

        public bool NewVersionVisible
        {
            get => this.newVersionVisible;
            set
            {
                if (this.newVersionVisible == value) return;

                this.newVersionVisible = value;

                RaisePropertyChanged();
            }
        }

        public string BlockStatus
        {
            get => this.blockStatus;
            set
            {
                this.blockStatus = value;
                RaisePropertyChanged(nameof(this.BlockStatus));
            }
        }
        public ICommand CreateWalletCommand => new RelayCommand(this.CreateWallet);

        public ICommand OpenWalletCommand => new RelayCommand(this.OpenWallet);

        public ICommand CloseWalletCommand => new RelayCommand(() => this.walletController.CloseWallet());

        public ICommand ChangePasswordCommand => new RelayCommand(() => this.dialogManager.ShowDialog<ChangePasswordDialogResult>());

        public ICommand ExitCommand => new RelayCommand(() => this.messagePublisher.Publish(new ExitAppMessage()));

        public ICommand TransferCommand => new RelayCommand(() => this.dialogManager.ShowDialog<TransferDialogResult>());

        public ICommand ShowTransactionDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<TradeDialogResult>());

        public ICommand ShowSigningDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<SigningDialogResult>());

        public ICommand ClaimCommand => new RelayCommand(() => this.dialogManager.ShowDialog<ClaimDialogResult>());

        public ICommand RequestCertificateCommand => new RelayCommand(() => this.dialogManager.ShowDialog<CertificateApplicationDialogResult>());

        public ICommand AssetRegistrationCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AssetRegistrationDialogResult>());

        public ICommand DistributeAssetCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AssetDistributionDialogResult>());

        public ICommand DeployContractCommand => new RelayCommand(() => this.dialogManager.ShowDialog<DeployContractDialogResult>());

        public RelayCommand InvokeContractCommand => new RelayCommand(() => this.messagePublisher.Publish(new InvokeContractMessage(null)));

        public ICommand ShowElectionDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<ElectionDialogResult>());

        public ICommand ShowSettingsCommand => new RelayCommand(() => this.dialogManager.ShowDialog<SettingsDialogResult>());

        public ICommand CheckForHelpCommand => new RelayCommand(() => { });

        public ICommand ShowOfficialWebsiteCommand => new RelayCommand(() => this.processHelper.OpenInExternalBrowser(OfficialWebsiteUrl));

        public RelayCommand ShowDeveloperToolsCommand => new RelayCommand(() => this.dialogManager.ShowDialog<DeveloperToolsDialogResult>());

        public ICommand AboutNeoCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AboutDialogResult>());

        public ICommand ShowUpdateDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<UpdateDialogResult>());
        #endregion Public Properies

        #region Constructor
        public HomeViewModel(
            IWalletController walletController,
            IDialogManager dialogManager,
            IProcessHelper processHelper,
            ISettingsManager settingsManager,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber)
        {
            this.walletController = walletController;
            this.dialogManager = dialogManager;
            this.processHelper = processHelper;
            this.settingsManager = settingsManager;
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<HomeDialogResult> SetDialogResultAndClose;

        public HomeDialogResult DialogResult { get; set; }
        #endregion

        #region ILoadable Implementation 
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion

        #region IUnloadable Implementation 
        public void OnUnload()
        {
            this.messageSubscriber.Unsubscribe(this);
        }
        #endregion

        #region IMessageHandler implementation 

        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            RaisePropertyChanged(nameof(this.WalletIsOpen));
        }

        public void HandleMessage(InvokeContractMessage message)
        {
            this.dialogManager.ShowDialog<InvokeContractDialogResult, InvokeContractLoadParameters>(
                new InvokeContractLoadParameters(message.Transaction));
        }

        public void HandleMessage(UpdateApplicationMessage message)
        {
            // Start update
            this.processHelper.Run(message.UpdateScriptPath);

            this.messagePublisher.Publish(new ExitAppMessage());
        }

        public void HandleMessage(NewVersionAvailableMessage message)
        {
            this.NewVersionLabel = $"{Strings.DownloadNewVersion}: {message.NewVersion}";
            this.NewVersionVisible = true;
        }

        public void HandleMessage(WalletStatusMessage message)
        {
            var status = message.Status;

            // TODO
            this.HeightStatus = $"{status.WalletHeight}/{status.BlockchainStatus.Height}/{status.BlockchainStatus.HeaderHeight}";
            this.NextBlockProgressIsIndeterminate = status.BlockchainStatus.NextBlockProgressIsIndeterminate;
            this.NextBlockProgressFraction = status.BlockchainStatus.NextBlockProgressFraction;

            this.NodeCount = status.NetworkStatus.NodeCount;
            this.BlockStatus = $"{Strings.WaitingForNextBlock}:"; // TODO Update property to return actual status
        }
        #endregion

        #region Private Methods 
        private void CreateWallet()
        {
            var result = this.dialogManager.ShowDialog<CreateWalletDialogResult>();

            if (result == null) return;

            if (string.IsNullOrEmpty(result.WalletPath) || string.IsNullOrEmpty(result.Password)) return;

            this.walletController.CreateWallet(result.WalletPath, result.Password);

            this.settingsManager.LastWalletPath = result.WalletPath;
            this.settingsManager.Save();
        }

        private void OpenWallet()
        {
            var result = this.dialogManager.ShowDialog<OpenWalletDialogResult>();

            if (result == null) return;

            var walletPath = result.WalletPath;
            var password = result.Password;

            if (string.IsNullOrEmpty(walletPath) || string.IsNullOrEmpty(password)) return;

            if (this.walletController.WalletCanBeMigrated(walletPath))
            {
                var migrationApproved = this.dialogManager.ShowMessageDialog(Strings.MigrateWalletCaption, Strings.MigrateWalletMessage, MessageDialogType.YesNo);

                if (migrationApproved == MessageDialogResult.Yes)
                {
                    // Try migrate wallet
                    var newWalletPath = this.walletController.MigrateWallet(walletPath, password);

                    if (!string.IsNullOrEmpty(newWalletPath))
                    {
                        walletPath = newWalletPath;
                    }
                }
            }

            this.walletController.OpenWallet(walletPath, password);

            this.settingsManager.LastWalletPath = walletPath;
            this.settingsManager.Save();
        }
        
        #endregion
    }
}