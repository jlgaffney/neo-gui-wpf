using System.Collections.Generic;
using Neo.Implementations.Wallets.EntityFramework;

namespace Neo.UI.Messages
{
    public class UpdateTransactionsMessage
    {
        public IEnumerable<TransactionInfo> Transactions { get; private set; }

        public UpdateTransactionsMessage(IEnumerable<TransactionInfo> transactions)
        {
            this.Transactions = transactions;
        }
    }
}
