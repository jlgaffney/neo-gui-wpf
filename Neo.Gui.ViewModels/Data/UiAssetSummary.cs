using GalaSoft.MvvmLight;
using Neo.UI.Core.Data.Enums;

namespace Neo.Gui.ViewModels.Data
{
    public class UiAssetSummary : ObservableObject
    {
        private string totalBalance;

        #region Public properties

        public string AssetId { get; }

        public string Name { get; }

        public string Issuer { get; }

        public string Type { get; }

        public TokenType TokenType { get; }

        public bool IsSystemAsset { get; }

        public string TotalBalance
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

        public UiAssetSummary(string assetId, string name, string issuer, string type, TokenType tokenType, bool isSystemAsset, string totalBalance)
        {
            this.AssetId = assetId;
            this.Name = name;
            this.Issuer = issuer;
            this.Type = type;
            this.TokenType = tokenType;
            this.IsSystemAsset = isSystemAsset;

            this.TotalBalance = totalBalance;
        }

        #endregion
    }
}
