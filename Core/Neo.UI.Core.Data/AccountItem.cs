using Neo.UI.Core.Data.Enums;
using Neo.Wallets;

namespace Neo.UI.Core.Data
{
    public class AccountItem : BindableClass
    {
        #region Private Fields 
        private Fixed8 neo;
        private Fixed8 gas;
        #endregion

        #region Public Properties 
        public string Label { get; }

        public string ScriptHash { get; }

        public string Address => Wallet.ToAddress(UInt160.Parse(this.ScriptHash));

        public AccountType Type { get; }

        public Fixed8 Neo
        {
            get => this.neo;
            set
            {
                if (this.neo == value) return;

                this.neo = value;

                NotifyPropertyChanged();
            }
        }

        public Fixed8 Gas
        {
            get => this.gas;
            set
            {
                if (this.gas == value) return;

                this.gas = value;

                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Constructor 
        public AccountItem(string label, string scriptHash, AccountType accountType)
        {
            this.Label = label;
            this.ScriptHash = scriptHash;
            this.Type = accountType;

            this.neo = Fixed8.Zero;
            this.gas = Fixed8.Zero;
        }
        #endregion
    }
}
