using Neo.UI.Base.MVVM;
using Neo.Wallets;

namespace Neo.UI
{
    public class AccountItem : ViewModelBase
    {
        private string address;
        private AccountType type;
        private Fixed8 neo;
        private Fixed8 gas;

        public string Address
        {
            get => this.address;
            set
            {
                if (this.address == value) return;

                this.address = value;

                NotifyPropertyChanged();
            }
        }

        public AccountType Type
        {
            get => this.type;
            set
            {
                if (this.type == value) return;

                this.type = value;

                NotifyPropertyChanged();
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

        public UInt160 ScriptHash { get; set; }

        public VerificationContract Contract { get; set; }
    }
}