using Neo.UI.Core.Data.TransactionParameters;
using Neo.UI.Core.Transactions.Testing;

namespace Neo.UI.Core.Transactions.Interfaces
{
    public interface ITransactionBuilder
    {
        ITransactionConfiguration Configuration { get; set; }

        bool IsContractTransaction { get; set; }

        bool IsValid(InvocationTransactionType invocationTransactionType);

        void GenerateTransaction();

        string GetTransactionScript();

        void SignAndRelayTransaction();

        void Invoke();
    }
}
