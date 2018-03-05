using System;
using Neo.Core;
using Neo.UI.Core.Helpers;

namespace Neo.UI.Core.Wallet.Data
{
    internal class TransactionItem
    {
        #region Public Properties 
        public UInt256 Hash { get; }

        public uint Height { get; }

        public DateTime Time { get; }

        public TransactionType Type { get; }

        public uint Confirmations { get; set; }
        #endregion

        #region Constructor 
        public TransactionItem(
            UInt256 transactionHash, 
            TransactionType transactionType, 
            uint height, 
            DateTime time)
        {
            Guard.ArgumentIsNotNull(transactionHash, () => transactionHash);

            this.Hash = transactionHash;
            this.Type = transactionType;
            this.Height = height;
            this.Time = time;
        }
        #endregion

        #region Public Methods 

        public override int GetHashCode()
        {
            return this.Hash.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var transactionItem = obj as TransactionItem;
            
            if (transactionItem == null) return false;

            return this.Hash.Equals(transactionItem.Hash);
        }
        #endregion
    }
}