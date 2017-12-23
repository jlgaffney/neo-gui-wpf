using System;
using Neo.Core;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Base.Data
{
    public class TransactionItem : BindableClass
    {
        private readonly Transaction transaction;

        private int confirmations;

        public TransactionItem(Transaction transaction, uint height, DateTime time)
        {
            this.transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            this.Height = height;
            this.Time = time;
        }
        
        public string Id => this.transaction.Hash.ToString();

        public uint Height { get; }

        public DateTime Time { get; }

        public TransactionType Type => this.transaction.Type;

        public string Confirmations => this.confirmations > 0 ? this.confirmations.ToString() : Strings.Unconfirmed;

        public void SetConfirmations(int value)
        {
            if (this.confirmations == value) return;

            if (this.confirmations < 0) value = 0;

            this.confirmations = value;

            NotifyPropertyChanged(nameof(this.Confirmations));
        }

        public override int GetHashCode()
        {
            return this.transaction.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var transactionItem = obj as TransactionItem;
            
            if (transactionItem == null) return false;

            return this.transaction.Hash.Equals(transactionItem.transaction.Hash);
        }
    }
}