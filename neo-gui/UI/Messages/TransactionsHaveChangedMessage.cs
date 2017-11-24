using System.Collections.Generic;

namespace Neo.UI.Messages
{
    public class TransactionsHaveChangedMessage
    {
        public IEnumerable<TransactionItem> Transactions { get; private set; }

        public TransactionsHaveChangedMessage(IEnumerable<TransactionItem> transactions)
        {
            this.Transactions = transactions;
        }
    }
}