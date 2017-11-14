using Neo.Core;

namespace Neo.UI.Messages
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