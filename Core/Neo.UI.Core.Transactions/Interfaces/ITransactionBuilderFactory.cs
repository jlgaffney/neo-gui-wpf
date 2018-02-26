using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Interfaces
{
    public interface ITransactionBuilderFactory
    {
        ITransactionBuilder<TParameters> GetBuilder<TParameters>(TParameters parameters)
            where TParameters : TransactionParameters;
    }
}
