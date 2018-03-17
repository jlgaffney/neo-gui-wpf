using System;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Messages
{
    public class TransactionAddedMessage
    {
        public string TransactionId { get; }

        public DateTime TransactionTime { get; }

        public uint? TransactionHeight { get; }

        public string TransactionType { get; }

        public TransactionAddedMessage(string transactionId, DateTime transactionTime, uint? transactionHeight, string transactionType)
        {
            this.TransactionId = transactionId;
            this.TransactionTime = transactionTime;
            this.TransactionHeight = transactionHeight;
            this.TransactionType = transactionType;
        }
    }
}
