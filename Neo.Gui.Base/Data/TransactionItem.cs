using System;
using Neo.Core;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Base.Helpers;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Base.Data
{
    public class TransactionItem : BindableClass
    {
        #region Private Fields 
        private int confirmations;
        #endregion

        #region Public Properties 
        public UInt256 Hash { get; private set; }

        public uint Height { get; private set; }

        public DateTime Time { get; private set; }

        public TransactionType Type { get; private set; }

        public string Confirmations => this.confirmations > 0 ? this.confirmations.ToString() : Strings.Unconfirmed;
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
        public void SetConfirmations(int value)
        {
            if (this.confirmations == value) return;

            if (this.confirmations < 0) value = 0;

            this.confirmations = value;

            NotifyPropertyChanged(nameof(this.Confirmations));
        }

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