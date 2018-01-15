using Neo.Wallets;

namespace Neo.UI.Core.Data
{
    public class AccountItem : BindableClass
    {
        private Fixed8 neo;
        private Fixed8 gas;

        public AccountItem(string label, UInt160 scriptHash, AccountType accountType)
        {
            this.Label = label;
            this.ScriptHash = scriptHash;
            this.Type = accountType;

            this.neo = Fixed8.Zero;
            this.gas = Fixed8.Zero;
        }
        
        public string Label { get; }

        public UInt160 ScriptHash { get; }

        public string Address => Wallet.ToAddress(this.ScriptHash);

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
    }
}
