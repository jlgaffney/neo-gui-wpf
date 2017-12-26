using System;

namespace Neo.Gui.Base.Status
{
    public class BlockchainStatus
    {
        public BlockchainStatus(uint height, uint headerHeight, bool nextBlockProgressIsIndeterminate,
            double nextBlockProgressFraction, TimeSpan timeSinceLastBlock)
        {
            this.Height = height;
            this.HeaderHeight = headerHeight;

            this.NextBlockProgressIsIndeterminate = nextBlockProgressIsIndeterminate;
            this.NextBlockProgressFraction = nextBlockProgressFraction;

            this.TimeSinceLastBlock = timeSinceLastBlock;
        }

        public uint Height { get; }

        public uint HeaderHeight { get; }

        public bool NextBlockProgressIsIndeterminate { get; }

        public double NextBlockProgressFraction { get; }

        public TimeSpan TimeSinceLastBlock { get; set; }
    }
}
