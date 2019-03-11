using ReactiveUI;
using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Messaging;
using Neo.Gui.Cross.Services;
using Neo.Gui.Cross.ViewModels.Wallets;

namespace Neo.Gui.Cross.ViewModels.Home
{
    public class WalletViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<WalletOpenedMessage>,
        IMessageHandler<WalletClosedMessage>
    {
        private readonly IMessageAggregator messageAggregator;
        private readonly IWalletService walletService;
        private readonly IWindowService windowService;

        public WalletViewModel() { }
        public WalletViewModel(
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
    }
}
