namespace Neo.UI.Messages
{
    public class BlockProgressMessage
    {
        public bool BlockProgressIndeterminate { get; private set; }

        public int BlockProgress { get; private set; }

        public string BlockHeight { get; private set; }

        public int NodeCount { get; private set; }

        public string BlockStatus { get; set; }

        public BlockProgressMessage(bool blockProgressIndeterminate, int blockProgress, string blockHeight, int nodeCount, string blockStatus)
        {
            this.BlockProgressIndeterminate = blockProgressIndeterminate;
            this.BlockProgress = blockProgress;
            this.BlockHeight = blockHeight;
            this.NodeCount = nodeCount;
            this.BlockStatus = blockStatus;
        }
    }
}
