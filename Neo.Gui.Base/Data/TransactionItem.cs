using System.Globalization;
using Neo.Core;
using Neo.Gui.Base.MVVM;
using Neo.Implementations.Wallets.EntityFramework;
//using Neo.Gui.Wpf.Properties;
using Neo.UI.Base.MVVM;

namespace Neo.UI
{
    public class TransactionItem : BindableClass
    {
        private int confirmations;

        public string Time => this.Info?.Time.ToString(CultureInfo.CurrentUICulture);

        public string Id => this.Info == null ? string.Empty : this.Info.Transaction.Hash.ToString();

        public string Confirmations => this.confirmations > 0 ? this.confirmations.ToString() : string.Empty;//Strings.Unconfirmed;

        public string Type => this.Info == null ? string.Empty : TransactionTypeToString(this.Info.Transaction.Type);

        public TransactionInfo Info { get; set; }

        public void SetConfirmations(int value)
        {
            if (this.confirmations == value) return;

            if (this.confirmations < 0) value = 0;

            this.confirmations = value;

            NotifyPropertyChanged(nameof(this.Confirmations));
        }

        private static string TransactionTypeToString(TransactionType type)
        {
            return type.ToString();
        }
    }
}