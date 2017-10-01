using Neo.Core;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Properties;

namespace Neo.UI.Models
{
    public class TransactionItem
    {
        private int confirmations;

        public TransactionInfo Info { get; set; }

        public string Id => this.Info?.Transaction.Hash.ToString();

        public string Time => this.Info?.Time.ToString();

        public string Type => this.Info == null ? string.Empty : TransactionTypeToString(this.Info.Transaction.Type);

        public string Confirmations => this.confirmations > 0
            ? this.confirmations.ToString()
                : Strings.Unconfirmed;

        public void SetConfirmations(int value)
        {
            if (confirmations < 0) value = 0;

            this.confirmations = value;
        }

        private string TransactionTypeToString(TransactionType type)
        {
            return type.ToString();
        }
    }
}