using GalaSoft.MvvmLight;

namespace Neo.Gui.ViewModels.Data
{
    public class UiAssetSummary : ViewModelBase
    {
        private decimal totalBalance;

        #region Public properties

        public string Name { get; }

        public string Issuer { get; }

        public string Type { get; }

        public decimal TotalBalance
        {
            get => this.totalBalance;
            set
            {
                if (this.totalBalance == value) return;
                this.totalBalance = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public UiAssetSummary(string name, string issuer, string type)
        {
            this.Name = name;
            this.Issuer = issuer;
            this.Type = type;
        }

        #endregion
    }
}
