using Neo.Core;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Interfaces
{
    public interface ITransactionBuilder
    {
    }

    public interface ITransactionBuilder<TParameters> : ITransactionBuilder
        where TParameters : TransactionParameters
    {
        Transaction Build(TParameters parameters);
    }
}
