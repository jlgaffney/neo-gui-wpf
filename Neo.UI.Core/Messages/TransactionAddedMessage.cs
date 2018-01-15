using Neo.UI.Core.Data;

namespace Neo.UI.Core.Messages
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
