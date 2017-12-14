using Neo.Core;

namespace Neo.Gui.Base.Dialogs.LoadParameters.Contracts
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
