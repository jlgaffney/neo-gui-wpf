using Neo.Core;

namespace Neo.Gui.Base.Messages
{
    public class SignTransactionAndShowInformationMessage
    {
        public SignTransactionAndShowInformationMessage(Transaction transaction)
        {
            this.Transaction = transaction;
        }

        public Transaction Transaction { get; }
    }
}