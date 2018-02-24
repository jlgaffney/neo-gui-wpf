namespace Neo.UI.Core.Data
{
    public class NetworkStatus
    {
        public int NodeCount { get; }

        public NetworkStatus(int nodeCount)
        {
            this.NodeCount = nodeCount;
        }
    }
}
