using System.Collections.Generic;
using Neo.Gui.Base.Data;

namespace Neo.Gui.Wpf.Messages
{
    public class TransactionsHaveChangedMessage
    {
        public TransactionsHaveChangedMessage(IEnumerable<TransactionItem> transactions)
        {
            this.Transactions = transactions;
        }

        public IEnumerable<TransactionItem> Transactions { get; }
    }
}