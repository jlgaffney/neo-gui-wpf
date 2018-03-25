using System.Globalization;
using GalaSoft.MvvmLight;
using Neo.UI.Core.Data.Enums;

namespace Neo.Gui.ViewModels.Data
{
    public class UiAssetSummary : ObservableObject
    {
        private string issuer;
        private string totalBalance;

        #region Public properties

        public string AssetId { get; }

        public string Name { get; }

        public string Issuer {
            get => this.issuer;
            set
            {
                if (this.issuer == value) return;
                this.issuer = value;
                RaisePropertyChanged();
            }
        }

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

        public UiAssetSummary(string assetId, string name, string issuer, string type, TokenType tokenType, bool isSystemAsset)
        {
            this.AssetId = assetId;
            this.Name = name;
            this.Issuer = issuer;
            this.Type = type;
            this.TokenType = tokenType;
            this.IsSystemAsset = isSystemAsset;
        }

        #endregion

        public void SetAssetBalance(decimal balance, decimal bonus)
        {
            var balanceStr = balance % 1 == 0
                ? ((int) balance).ToString(CultureInfo.CurrentUICulture)
                : balance.ToString(CultureInfo.CurrentUICulture);

            if (bonus > 0)
            {
                balanceStr += $"+({bonus})";
            }

            this.TotalBalance = balanceStr;
        }
    }
}
