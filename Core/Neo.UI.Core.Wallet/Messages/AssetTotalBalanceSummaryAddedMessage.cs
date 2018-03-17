namespace Neo.UI.Core.Wallet.Messages
{
    public class AssetTotalBalanceSummaryAddedMessage
    {
        public string AssetId { get; }

        public string AssetName { get; }

        public string AssetIssuer { get; }

        public string AssetType { get; }

        public bool IsSystemAsset { get; }


        public decimal TotalBalance { get; }

        public decimal TotalBonus { get; }

        public AssetTotalBalanceSummaryAddedMessage(
            string assetId, string assetName,
            string assetIssuer, string assetType,
            bool isSystemAsset, decimal totalBalance, decimal totalBonus)
        {
            this.AssetId = assetId;
            this.AssetName = assetName;
            this.AssetIssuer = assetIssuer;
            this.AssetType = assetType;
            this.IsSystemAsset = isSystemAsset;

            this.TotalBalance = totalBalance;
            this.TotalBonus = totalBonus;
        }
    }
}
