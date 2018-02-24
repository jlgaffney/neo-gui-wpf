using Neo.Core;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Builders
{
    internal class InvokeContractTransactionBuilder : ITransactionBuilder<InvokeContractTransactionParameters>
    {
        public Transaction Build(InvokeContractTransactionParameters parameters)
        {
            return new InvocationTransaction
            {
                Version = 1,
                Script = parameters.Script,

                Attributes = new TransactionAttribute[0],
                Inputs = new CoinReference[0],
                Outputs = new TransactionOutput[0],
                Scripts = new Witness[0]
            };
        }
    }
}
