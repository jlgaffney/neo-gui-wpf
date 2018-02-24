using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Messages
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
