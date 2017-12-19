using Neo.Core;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Base.Data
{
    public class AssetItem : BindableClass
    {
        private string name;
        private string type;
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

        public string Type
        {
            get => this.type;
            set
            {
                if (this.type == value) return;

                this.type = value;

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

        public bool IsSystemAsset => this.State != null &&
            (this.State.AssetType == AssetType.GoverningToken ||
                this.State.AssetType == AssetType.UtilityToken);

        public AssetState State { get; set; }

        public UInt160 ScriptHashNEP5 { get; set; }
    }
}