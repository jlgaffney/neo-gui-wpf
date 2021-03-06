﻿using System.Collections.Generic;

using Neo.Gui.Base.Data;

namespace Neo.Gui.Base.Messages
{
    public class TransactionsHaveChangedMessage
    {
        public IEnumerable<TransactionItem> Transactions { get; }

        public TransactionsHaveChangedMessage(IEnumerable<TransactionItem> transactions)
        {
            this.Transactions = transactions;
        }

    }
}