namespace Neo.UI.Core.Messages
{
    public class TransactionConfirmationsUpdatedMessage
    {
        public TransactionConfirmationsUpdatedMessage(uint blockHeight)
        {
            this.BlockHeight = blockHeight;
        }

        public uint BlockHeight { get; }
    }
}
