using System;
using Neo.Core;
using Neo.Gui.Globalization.Resources;
using Neo.UI.Core.Helpers;

namespace Neo.UI.Core.Data
{
    public class TransactionItem : BindableClass
    {
        #region Private Fields 
        private uint confirmations;
        #endregion

        #region Public Properties 
        public UInt256 Hash { get; }

        public uint Height { get; }

        public DateTime Time { get; }

        public TransactionType Type { get; }

        public uint Confirmations
        {
            get => this.confirmations;
            set
            {
                if (this.confirmations == value) return;

                this.confirmations = value;

                NotifyPropertyChanged();
            }
        }

        public string ConfirmationsText => this.Confirmations > 0 ? this.Confirmations.ToString() : Strings.Unconfirmed;
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