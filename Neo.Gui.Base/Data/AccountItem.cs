using Neo.Wallets;

using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Base.Data
{
    public class AccountItem : BindableClass
    {
        private Fixed8 neo;
        private Fixed8 gas;

        public string Address => this.Account.Address;

        public AccountType Type
        {
            get
            {
                if (this.Account.WatchOnly)
                {
                    return AccountType.WatchOnly;
                }

                return this.Account.Contract.IsStandard
                    ? AccountType.Standard
                    : AccountType.NonStandard;
            }
        }

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

        public WalletAccount Account { get; set; }
    }
}
