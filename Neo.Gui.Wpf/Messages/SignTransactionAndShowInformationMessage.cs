using Neo.Core;

namespace Neo.Gui.Wpf.Messages
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