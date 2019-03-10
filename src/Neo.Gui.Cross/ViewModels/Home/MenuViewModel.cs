using System.Diagnostics;
using Avalonia;
using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Messaging;
using Neo.Gui.Cross.Services;
using Neo.Gui.Cross.ViewModels.Assets;
using Neo.Gui.Cross.ViewModels.Contracts;
using Neo.Gui.Cross.ViewModels.Development;
using Neo.Gui.Cross.ViewModels.Settings;
using Neo.Gui.Cross.ViewModels.Transactions;
using Neo.Gui.Cross.ViewModels.Voting;
using Neo.Gui.Cross.ViewModels.Wallets;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Home
{
    public class MenuViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<WalletOpenedMessage>,
        IMessageHandler<WalletClosedMessage>
    {
        private const string WebsiteUrl = "https://neo.org/";

        private readonly IMessageAggregator messageAggregator;
        private readonly IWalletService walletService;
        private readonly IWindowService windowService;

        public MenuViewModel() { }
        public MenuViewModel(
            IMessageAggregator messageAggregator,
            IWalletService walletService,
            IWindowService windowService)
        {
            this.messageAggregator = messageAggregator;
            this.walletService = walletService;
            this.windowService = windowService;
        }

        public bool WalletIsOpen => walletService.WalletIsOpen;
        
        public ReactiveCommand NewWalletCommand => ReactiveCommand.Create(() => windowService.ShowDialog<NewWalletViewModel>());

        public ReactiveCommand OpenWalletCommand => ReactiveCommand.Create(() => windowService.ShowDialog<OpenWalletViewModel>());

        //public ReactiveCommand ChangeWalletPasswordCommand => ReactiveCommand.Create(() => windowService.ShowDialog<ChangeWalletPasswordViewModel>());

        public ReactiveCommand CloseWalletCommand => ReactiveCommand.Create(CloseWallet);

        public ReactiveCommand ExitApplicationCommand => ReactiveCommand.Create(() => Application.Current.Exit());

        public ReactiveCommand TransferCommand => ReactiveCommand.Create(() => windowService.ShowDialog<TransferViewModel>());

        public ReactiveCommand TransactionsCommand => ReactiveCommand.Create(() => windowService.ShowDialog<TradeViewModel>());

        public ReactiveCommand SignatureCommand => ReactiveCommand.Create(() => windowService.ShowDialog<SigningViewModel>());

        public ReactiveCommand GasClaimCommand => ReactiveCommand.Create(() => windowService.ShowDialog<GasClaimViewModel>());

        public ReactiveCommand RequestCertificateCommand => ReactiveCommand.Create(() => windowService.ShowDialog<CertificateRequestViewModel>());

        public ReactiveCommand AssetRegistrationCommand => ReactiveCommand.Create(() => windowService.ShowDialog<AssetRegistrationViewModel>());

        public ReactiveCommand AssetDistributionCommand => ReactiveCommand.Create(() => windowService.ShowDialog<AssetDistributionViewModel>());

        public ReactiveCommand DeployContractCommand => ReactiveCommand.Create(() => windowService.ShowDialog<DeployContractViewModel>());

        public ReactiveCommand InvokeContractCommand => ReactiveCommand.Create(() => windowService.ShowDialog<InvokeContractViewModel>());

        public ReactiveCommand ElectionCommand => ReactiveCommand.Create(() => windowService.ShowDialog<ElectionViewModel>());

        //public ReactiveCommand SignMessageCommand => ReactiveCommand.Create(() => windowService.ShowDialog<SignMessageViewModel>());

        public ReactiveCommand SettingsCommand => ReactiveCommand.Create(() => windowService.ShowDialog<SettingsViewModel>());

        public ReactiveCommand CheckForHelpCommand => ReactiveCommand.Create(() => {});

        public ReactiveCommand OfficialWebsiteCommand => ReactiveCommand.Create(() => Process.Start(WebsiteUrl));

        public ReactiveCommand DeveloperToolsCommand => ReactiveCommand.Create(() => windowService.Show<DeveloperToolsViewModel>());

        //public ReactiveCommand AboutApplicationCommand => ReactiveCommand.Create(() => windowService.ShowDialog<AboutApplicationViewModel>());*/

        public void Load()
        {
            messageAggregator.Subscribe(this);
        }

        public void Unload()
        {
            messageAggregator.Unsubscribe(this);
        }
        
        public void HandleMessage(WalletOpenedMessage message)
        {
            this.RaisePropertyChanged(nameof(WalletIsOpen));
        }

        public void HandleMessage(WalletClosedMessage message)
        {
            this.RaisePropertyChanged(nameof(WalletIsOpen));
        }

        private void CloseWallet()
        {
            walletService.CloseWallet();
        }
    }
}
