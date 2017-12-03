namespace Neo.Gui.Base.Controllers
{
    public class WalletStatus
    {
        public WalletStatus(uint walletHeight, uint blockChainHeight, uint blockChainHeaderHeight,
            bool nextBlockProgressIsIndeterminate, double nextBlockProgressFraction, uint nodeCount)
        {
            this.WalletHeight = walletHeight;

            this.BlockChainHeight = blockChainHeight;
            this.BlockChainHeaderHeight = blockChainHeaderHeight;

            this.NextBlockProgressIsIndeterminate = nextBlockProgressIsIndeterminate;
            this.NextBlockProgressFraction = nextBlockProgressFraction;

            this.NodeCount = nodeCount;
        }

        public uint WalletHeight { get; }

        public uint BlockChainHeight { get; }

        public uint BlockChainHeaderHeight { get; }
    
        public bool NextBlockProgressIsIndeterminate { get; }

        public double NextBlockProgressFraction { get; }

        public uint NodeCount { get; }
    }
}
