using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Data
{
    public class AccountBalanceSummary : BindableClass
    {
        #region Private Fields 
        private Fixed8 neo;
        private Fixed8 gas;
        #endregion

        public AccountBalanceSummary()
        {
            this.neo = Fixed8.Zero;
            this.gas = Fixed8.Zero;
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
    }
}
