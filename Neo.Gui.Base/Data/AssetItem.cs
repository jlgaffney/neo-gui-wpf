using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Base.Data
{
    public abstract class AssetItem : BindableClass
    {
        private string name;
        private string value;
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

        public string Value
        {
            get => this.value;
            set
            {
                if (this.value == value) return;

                this.value = value;

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

        public abstract string Type { get; }
    }
}
