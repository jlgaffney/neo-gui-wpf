using Neo.Core;
using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.UI.Core.Transactions.Builders
{
    internal class InvokeTransactionBuilder : TransactionBuilderBase
    {
        public override bool IsValid(InvocationTransactionType invocationTransactionType)
        {
            return invocationTransactionType == InvocationTransactionType.Invoke;
        }

        public override void GenerateTransaction()
        {
            this.IsContractTransaction = true;

            var invokeTransactionConfiguration = this.Configuration as InvokeTransactionConfiguration;
            if (invokeTransactionConfiguration.InvokeTransactionParameters == null)
            {
                return;
            }

            this.Transaction = new InvocationTransaction();
        }
    }
}
