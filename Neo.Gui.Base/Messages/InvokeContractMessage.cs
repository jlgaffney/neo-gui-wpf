using Neo.Core;

namespace Neo.Gui.Base.Messages
{
    public class InvokeContractMessage
    {
        public InvokeContractMessage(InvocationTransaction transaction)
        {
            this.Transaction = transaction;
        }

        public InvocationTransaction Transaction { get; }
    }
}