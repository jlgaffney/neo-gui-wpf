using Neo.Gui.Base.Data;

namespace Neo.Gui.Base.Messages
{
    public class TransactionAddedMessage
    {
        public TransactionAddedMessage(TransactionItem transaction)
        {
            this.Transaction = transaction;
        }

        public TransactionItem Transaction { get; }
    }
}
