namespace Neo.UI.Core.Wallet.Messages
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
