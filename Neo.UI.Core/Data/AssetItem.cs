namespace Neo.UI.Core.Data
{
    public abstract class AssetItem : BindableClass
    {
        private string name;
        private string issuer;

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

        public abstract string Value { get; }

        public abstract string Type { get; }
    }
}
