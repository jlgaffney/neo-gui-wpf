namespace Neo.UI.Core.Wallet.Messages
{
    public class AssetTotalBalanceChangedMessage
    {
        public string AssetId { get; }

        public decimal TotalBalance { get; }

        public decimal TotalBonus { get; }

        public AssetTotalBalanceChangedMessage(string assetId, decimal totalBalance, decimal totalBonus)
        {
            this.AssetId = assetId;
            this.TotalBalance = totalBalance;
            this.TotalBonus = totalBonus;
        }
    }
}
