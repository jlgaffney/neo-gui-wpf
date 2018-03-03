namespace Neo.UI.Core.Data
{
    public abstract class AssetSummary : BindableClass
    {
        private string name;
        private string issuer;
        private string totalBalance;

        public string Name
        {
            get => this.name;
            set
            {
                if (this.name == value) return;

                this.name = value;

                NotifyPropertyChanged();
            }
        }

        public string Issuer
        {
            get => this.issuer;
            set
            {
                if (this.issuer == value) return;

                this.issuer = value;

                NotifyPropertyChanged();
            }
        }

        public string TotalBalance
        {
            get => this.totalBalance;
            set
            {
                if (this.totalBalance == value) return;

                this.totalBalance = value;

                NotifyPropertyChanged();
            }
        }

        public abstract string Type { get; }
    }
}
