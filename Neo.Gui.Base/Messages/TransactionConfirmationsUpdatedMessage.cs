namespace Neo.Gui.Base.Messages
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
