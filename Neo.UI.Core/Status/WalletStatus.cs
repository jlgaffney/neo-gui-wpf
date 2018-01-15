namespace Neo.UI.Core.Status
{
    public class WalletStatus
    {
        public WalletStatus(uint walletHeight, BlockchainStatus blockchainStatus, NetworkStatus networkStatus)
        {
            this.WalletHeight = walletHeight;

            this.BlockchainStatus = blockchainStatus;

            this.NetworkStatus = networkStatus;
        }

        public uint WalletHeight { get; }

        public BlockchainStatus BlockchainStatus { get; }

        public NetworkStatus NetworkStatus { get; }
    }
}
