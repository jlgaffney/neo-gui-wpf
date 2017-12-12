using System;

namespace Neo.Gui.Base.Controllers
{
    public class BlockchainStatus
    {
        public BlockchainStatus(uint height, uint headerHeight, bool nextBlockProgressIsIndeterminate,
            double nextBlockProgressFraction, TimeSpan timeSinceLastBlock, uint nodeCount)
        {
            this.Height = height;
            this.HeaderHeight = headerHeight;

            this.NextBlockProgressIsIndeterminate = nextBlockProgressIsIndeterminate;
            this.NextBlockProgressFraction = nextBlockProgressFraction;

            this.TimeSinceLastBlock = timeSinceLastBlock;

            this.NodeCount = nodeCount;
        }

        public uint Height { get; }

        public uint HeaderHeight { get; }

        public bool NextBlockProgressIsIndeterminate { get; }

        public double NextBlockProgressFraction { get; }

        public TimeSpan TimeSinceLastBlock { get; set; }

        public uint NodeCount { get; set; }
    }
}
