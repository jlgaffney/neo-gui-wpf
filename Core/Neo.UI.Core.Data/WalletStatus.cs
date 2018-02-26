namespace Neo.UI.Core.Data
{
    public class WalletStatus
    {
        public WalletStatus(uint walletHeight, BlockchainStatus blockchainStatus)
        {
            this.WalletHeight = walletHeight;

            this.BlockchainStatus = blockchainStatus;
        }

        public uint WalletHeight { get; }

        public BlockchainStatus BlockchainStatus { get; }
    }
}
