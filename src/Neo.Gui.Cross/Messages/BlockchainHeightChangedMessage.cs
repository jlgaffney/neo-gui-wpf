namespace Neo.Gui.Cross.Messages
{
    public class BlockchainHeightChangedMessage
    {
        public BlockchainHeightChangedMessage(uint height)
        {
            Height = height;
        }

        public uint Height { get; }
    }
}
