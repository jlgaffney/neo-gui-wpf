using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.UI.Core.Globalization.Resources;
using Neo.Gui.Dialogs;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters;
using Neo.Gui.Dialogs.LoadParameters.Assets;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.LoadParameters.Development;
using Neo.Gui.Dialogs.LoadParameters.Home;
using Neo.Gui.Dialogs.LoadParameters.Settings;
using Neo.Gui.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Dialogs.LoadParameters.Updater;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.Gui.Dialogs.LoadParameters.Wallets;
using Neo.Gui.Dialogs.Results.Wallets;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Wallet;
using Neo.UI.Core.Wallet.Messages;

namespace Neo.Gui.ViewModels.Home
{
    public class HomeViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IDialogViewModel<HomeLoadParameters>,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<WalletStatusMessage>
    {
        #region Private Fields
        private const string OfficialWebsiteUrl = "https://neo.org/";

        private readonly IDialogManager dialogManager;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IProcessManager processManager;
        private readonly ISettingsManager settingsManager;
        private readonly IVersionService versionService;
        private readonly IWalletController walletController;

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
        public RelayCommand CreateWalletCommand => new RelayCommand(this.CreateWallet);

        public RelayCommand OpenWalletCommand => new RelayCommand(this.OpenWallet);

        public RelayCommand CloseWalletCommand => new RelayCommand(() => this.walletController.CloseWallet());

        public RelayCommand ExitCommand => new RelayCommand(() => this.processManager.Exit());

        public RelayCommand TransferCommand => new RelayCommand(() => this.dialogManager.ShowDialog<TransferLoadParameters>());

        public RelayCommand ShowTransactionDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<TradeLoadParameters>());

        public RelayCommand ShowSigningDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<SigningLoadParameters>());

        public RelayCommand ClaimCommand => new RelayCommand(() => this.dialogManager.ShowDialog<ClaimLoadParameters>());

        public RelayCommand RequestCertificateCommand => new RelayCommand(() => this.dialogManager.ShowDialog<CertificateRequestLoadParameters>());

        public RelayCommand AssetRegistrationCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AssetRegistrationLoadParameters>());

        public RelayCommand DistributeAssetCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AssetDistributionLoadParameters>());

        public RelayCommand DeployContractCommand => new RelayCommand(() => this.dialogManager.ShowDialog<DeployContractLoadParameters>());

        public RelayCommand InvokeContractCommand => new RelayCommand(() => this.dialogManager.ShowDialog(new InvokeContractLoadParameters(null)));

        public RelayCommand ShowElectionDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<ElectionLoadParameters>());

        public RelayCommand ShowSettingsCommand => new RelayCommand(() => this.dialogManager.ShowDialog<SettingsLoadParameters>());

        public RelayCommand CheckForHelpCommand => new RelayCommand(() => { });

        public RelayCommand ShowOfficialWebsiteCommand => new RelayCommand(() => this.processManager.OpenInExternalBrowser(OfficialWebsiteUrl));

        public RelayCommand ShowDeveloperToolsCommand => new RelayCommand(() => this.dialogManager.ShowDialog<DeveloperToolsLoadParameters>());

        public RelayCommand AboutNeoCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AboutLoadParameters>());

        public RelayCommand ShowUpdateDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<UpdateLoadParameters>());
        #endregion Public Properies

        #region Constructor
        public HomeViewModel(
            IDialogManager dialogManager,
            IMessageSubscriber messageSubscriber,
            IProcessManager processManager,
            ISettingsManager settingsManager,
            IVersionService versionService,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.messageSubscriber = messageSubscriber;
            this.processManager = processManager;
            this.settingsManager = settingsManager;
            this.versionService = versionService;
            this.walletController = walletController;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(HomeLoadParameters parameters)
        {
        }
        #endregion

        #region ILoadable Implementation 
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);

            this.CheckForNewerApplicationVersion();

            this.walletController.Refresh();
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

        public void HandleMessage(WalletStatusMessage message)
        {
            this.HeightStatus = this.settingsManager.LightWalletMode ? message.BlockchainStatus.Height.ToString() : $"{message.WalletHeight}/{message.BlockchainStatus.Height}/{message.BlockchainStatus.HeaderHeight}";
            this.NextBlockProgressIsIndeterminate = message.BlockchainStatus.NextBlockProgressIsIndeterminate;
            this.NextBlockProgressFraction = message.BlockchainStatus.NextBlockProgressFraction;

            this.NodeCount = message.BlockchainStatus.NodeCount;
            this.BlockStatus = $"{Strings.WaitingForNextBlock}:"; // TODO Update property to return actual status
        }
        #endregion

        #region Private Methods 
        private void CheckForNewerApplicationVersion()
        {
            var latestVersion = this.versionService.LatestVersion;
            var currentVersion = this.versionService.CurrentVersion;
            if (latestVersion != null && currentVersion != null && latestVersion > currentVersion)
            {
                // Newer version is available
                this.NewVersionLabel = $"{Strings.DownloadNewVersion}: {latestVersion}";
                this.NewVersionVisible = true;
            }
        }

        private void CreateWallet()
        {
            var result = this.dialogManager.ShowDialog<CreateWalletLoadParameters, CreateWalletDialogResult>();

            if (result == null) return;

            if (string.IsNullOrEmpty(result.WalletPath) || string.IsNullOrEmpty(result.Password)) return;

            this.walletController.CreateWallet(result.WalletPath, result.Password);

            this.settingsManager.LastWalletPath = result.WalletPath;
            this.settingsManager.Save();
        }

        private void OpenWallet()
        {
            var result = this.dialogManager.ShowDialog<OpenWalletLoadParameters, OpenWalletDialogResult>();

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

                    if (string.IsNullOrEmpty(newWalletPath)) return;
                    
                    walletPath = newWalletPath;
                }
            }

            this.walletController.OpenWallet(walletPath, password);

            this.settingsManager.LastWalletPath = walletPath;
            this.settingsManager.Save();
        }
        
        #endregion
    }
}