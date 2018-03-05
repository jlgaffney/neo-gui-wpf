using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Messages
{
    public class WalletStatusMessage
    {
        public WalletStatusMessage(uint walletHeight, BlockchainStatus blockchainStatus)
        {
            this.WalletHeight = walletHeight;
            this.BlockchainStatus = blockchainStatus;
        }

        public uint WalletHeight { get; }

        public BlockchainStatus BlockchainStatus { get; }
    }
}
