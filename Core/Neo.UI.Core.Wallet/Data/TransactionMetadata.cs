using System;
using Neo.Core;
using Neo.UI.Core.Helpers;

namespace Neo.UI.Core.Wallet.Data
{
    internal class TransactionMetadata
    {
        #region Public Properties 
        public UInt256 Id { get; }

        public uint? Height { get; }

        public DateTime Time { get; }

        public TransactionType Type { get; }
        #endregion

        #region Constructor 
        public TransactionMetadata(
            UInt256 transactionId, 
            TransactionType transactionType, 
            uint? height, 
            DateTime time)
        {
            Guard.ArgumentIsNotNull(transactionId, () => transactionId);

            this.Id = transactionId;
            this.Type = transactionType;
            this.Height = height;
            this.Time = time;
        }
        #endregion

        #region Public Methods 

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var transactionItem = obj as TransactionMetadata;
            
            if (transactionItem == null) return false;

            return this.Id.Equals(transactionItem.Id);
        }
        #endregion
    }
}