namespace Neo.UI.Core.Status
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
