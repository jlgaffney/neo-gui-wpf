using System.Collections.Generic;
using Neo.Gui.Cross.Models;

namespace Neo.Gui.Cross.Services
{
    public interface ITransactionService
    {
        IEnumerable<TransactionStateDetails> GetWalletTransactions();
    }
}
