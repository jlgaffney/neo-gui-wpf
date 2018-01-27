using Neo.UI.Core.Transactions.Testing;

namespace Neo.UI.Core.Transactions.Interfaces
{
    public interface ITransactionTester
    {
        TestForGasUsageResult TestForGasUsage(ITransactionBuilder builder, string customScript);
    }
}
