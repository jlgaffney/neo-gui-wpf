using System.Timers;
using Neo.Gui.Cross.Resources;
using Neo.Gui.Cross.Services;
using Neo.Ledger;
using Neo.Network.P2P;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Home
{
    public class StatusBarViewModel : ViewModelBase
    {
        private const int RefreshIntervalMilliseconds = 1500;

        private readonly IWalletService walletService;
        
        private string blockStatus;

        public StatusBarViewModel() { }
        public StatusBarViewModel(
            IWalletService walletService)
        {
            this.walletService = walletService;

            StartRefreshTimer();
        }

        public string HeightStatus => $"{(walletService.WalletIsOpen ? walletService.CurrentWallet.WalletHeight : 0)}/{Blockchain.Singleton.Height}/{Blockchain.Singleton.HeaderHeight}";
        
        public int ConnectedNodeCount => LocalNode.Singleton.ConnectedCount;

        public string BlockStatus
        {
            get => blockStatus;
            set
            {
                if (blockStatus == value)
                {
                    return;
                }

                blockStatus = value;

                this.RaisePropertyChanged();
            }
        }

        private void StartRefreshTimer()
        {
            var refreshTimer = new Timer(RefreshIntervalMilliseconds);
            refreshTimer.Elapsed += (sender, e) =>
            {
                Refresh();
            };
            refreshTimer.Enabled = true;
        }

        private void Refresh()
        {
            this.RaisePropertyChanged(nameof(HeightStatus));
            this.RaisePropertyChanged(nameof(ConnectedNodeCount));

            BlockStatus = Strings.WaitingForNextBlock;
        }
    }
}
