using Neo.UI.Core.Controllers.TransactionInvokers;
using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.UI.Core.Controllers
{
    public interface ITransactionInvoker
    {
        ITransactionConfiguration Configuration { get; set; }

        bool IsValid(InvocationTransactionType invocationTransactionType);

        void GenerateTransaction();

        string GetTransactionScript();

        TestForGasUsageResult TestForGasUsage(string customScript);

        void Invoke();
    }
}
