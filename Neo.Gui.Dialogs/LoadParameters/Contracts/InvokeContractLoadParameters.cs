using Neo.Core;

namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class InvokeContractLoadParameters
    {
        public InvocationTransaction Transaction { get; }

        public InvokeContractLoadParameters(InvocationTransaction transaction)
        {
            this.Transaction = transaction;
        }
    }
}
