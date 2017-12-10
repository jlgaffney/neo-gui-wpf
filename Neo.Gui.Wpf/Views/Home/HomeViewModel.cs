using System;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Contracts;
using Neo.Gui.Base.Dialogs.Results.Settings;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Dialogs.Results.Development;
using Neo.Gui.Base.Dialogs.Results.Home;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Services;

using Neo.Gui.Wpf.MVVM;
using Neo.Gui.Wpf.Views.Contracts;

namespace Neo.Gui.Wpf.Views.Home
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
        private readonly IDispatchService dispatchService;

        private bool nextBlockProgressIsIndeterminate;
        private double nextBlockProgressFraction;

        private string newVersionLabel;
        private bool newVersionVisible;

        private string heightStatus;
        private uint nodeCount;
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

                NotifyPropertyChanged();
            }
        }

        public uint NodeCount
        {
            get => this.nodeCount;
            set
            {
                if (this.nodeCount == value) return;

                this.nodeCount = value;

                NotifyPropertyChanged();
            }
        }

        public bool NextBlockProgressIsIndeterminate
        {
            get => this.nextBlockProgressIsIndeterminate;
            set
            {
                if (this.nextBlockProgressIsIndeterminate == value) return;

                this.nextBlockProgressIsIndeterminate = value;

                NotifyPropertyChanged();
            }
        }

        public double NextBlockProgressFraction
        {
            get => this.nextBlockProgressFraction;
            set
            {
                if (this.nextBlockProgressFraction.Equals(value)) return;

                this.nextBlockProgressFraction = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.NextBlockProgressPercentage));
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

        public string BlockStatus
        {
            get => this.blockStatus;
            set
            {
                this.blockStatus = value;
                this.NotifyPropertyChanged(nameof(this.BlockStatus));
            }
        }
        public RelayCommand CreateWalletCommand => new RelayCommand(this.CreateWallet);

        public RelayCommand OpenWalletCommand => new RelayCommand(this.OpenWallet);

        public RelayCommand CloseWalletCommand => new RelayCommand(() => this.walletController.CloseWallet());

        public RelayCommand ChangePasswordCommand => new RelayCommand(() => this.dialogManager.ShowDialog<ChangePasswordDialogResult>());

        public RelayCommand RebuildIndexCommand => new RelayCommand(this.RebuildIndex);

        public RelayCommand RestoreAccountsCommand => new RelayCommand(() => this.dialogManager.ShowDialog<RestoreAccountsDialogResult>());

        public RelayCommand ExitCommand => new RelayCommand(() => this.messagePublisher.Publish(new ExitAppMessage()));

        public RelayCommand TransferCommand => new RelayCommand(() => this.dialogManager.ShowDialog<TransferDialogResult>());

        public RelayCommand ShowTransactionDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<TradeDialogResult>());

        public RelayCommand ShowSigningDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<SigningDialogResult>());

        public RelayCommand ClaimCommand => new RelayCommand(() => this.dialogManager.ShowDialog<ClaimDialogResult>());

        public RelayCommand RequestCertificateCommand => new RelayCommand(() => this.dialogManager.ShowDialog<CertificateApplicationDialogResult>());

        public RelayCommand AssetRegistrationCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AssetRegistrationDialogResult>());

        public RelayCommand DistributeAssetCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AssetDistributionDialogResult>());

        public RelayCommand DeployContractCommand => new RelayCommand(() => this.dialogManager.ShowDialog<DeployContractDialogResult>());

        public RelayCommand InvokeContractCommand => new RelayCommand(InvokeContract);

        public RelayCommand ShowElectionDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<ElectionDialogResult>());

        public RelayCommand ShowSettingsCommand => new RelayCommand(() => this.dialogManager.ShowDialog<SettingsDialogResult>());

        public RelayCommand CheckForHelpCommand => new RelayCommand(() => { });

        public RelayCommand ShowOfficialWebsiteCommand => new RelayCommand(() => this.processHelper.OpenInExternalBrowser(OfficialWebsiteUrl));

        public RelayCommand ShowDeveloperToolsCommand => new RelayCommand(ShowDeveloperTools);

        public RelayCommand AboutNeoCommand => new RelayCommand(() => this.dialogManager.ShowDialog<AboutDialogResult>());

        public RelayCommand ShowUpdateDialogCommand => new RelayCommand(() => this.dialogManager.ShowDialog<UpdateDialogResult>());
        #endregion Public Properies

        #region Constructor
        public HomeViewModel(
            IWalletController walletController,
            IDialogManager dialogManager, 
            IProcessHelper processHelper,
            ISettingsManager settingsManager,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber, 
            IDispatchService dispatchService)
        {
            this.walletController = walletController;
            this.dialogManager = dialogManager;
            this.processHelper = processHelper;
            this.settingsManager = settingsManager;
            this.messagePublisher = messagePublisher;
            this.messageSubscriber = messageSubscriber;
            this.dispatchService = dispatchService;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<HomeDialogResult> SetDialogResultAndClose;

        public HomeDialogResult DialogResult { get; set; }
        #endregion

        #region ILoadable Implementation 
        public void OnLoad(params object[] parameters)
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
            this.NotifyPropertyChanged(nameof(this.WalletIsOpen));
        }

        public void HandleMessage(InvokeContractMessage message)
        {
            this.dialogManager.ShowDialog<InvokeContractDialogResult, InvokeContractLoadParameters>(
                new LoadParameters<InvokeContractLoadParameters>(new InvokeContractLoadParameters(message.Transaction)));
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
            this.HeightStatus = status.WalletHeight + "/" + status.BlockChainHeight + "/" + status.BlockChainHeaderHeight;
            this.NextBlockProgressIsIndeterminate = status.NextBlockProgressIsIndeterminate;
            this.NextBlockProgressFraction = status.NextBlockProgressFraction;

            this.NodeCount = status.NodeCount;
            this.BlockStatus = $"{Strings.WaitingForNextBlock}:"; // TODO Update property to return actual status
        }
        #endregion

        #region Private Methods 
        private void InvokeContract()
        {
            this.messagePublisher.Publish(new InvokeContractMessage(null));
        }

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

            if (string.IsNullOrEmpty(result.WalletPath) || string.IsNullOrEmpty(result.Password)) return;

            if (this.walletController.WalletNeedUpgrade(result.WalletPath))
            {
                //var migrationApproved = this.dialogManager.ShowDialog<YesOrNoDialogResult>("ApproveWalletMigrationDialog");

                //if (!migrationApproved.Yes) return;

                //this.walletController.UpgradeWallet(result.WalletPath);
            }

            this.walletController.OpenWallet(result.WalletPath, result.Password, result.OpenInRepairMode);

            this.settingsManager.LastWalletPath = result.WalletPath;
            this.settingsManager.Save();
        }

        private async void RebuildIndex()
        {
            await this.dispatchService.InvokeOnMainUIThread(() =>
            {
                this.messagePublisher.Publish(new ClearAssetsMessage());
                this.messagePublisher.Publish(new ClearTransactionsMessage());
            });

            this.walletController.RebuildCurrentWallet();
        }

        private void ShowDeveloperTools()
        {
            this.dialogManager.ShowDialog<DeveloperToolsDialogResult>();
        }
        #endregion
    }
}