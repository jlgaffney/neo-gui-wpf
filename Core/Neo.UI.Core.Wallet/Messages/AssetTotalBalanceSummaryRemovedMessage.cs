namespace Neo.UI.Core.Wallet.Messages
{
    public class AssetTotalBalanceSummaryRemovedMessage
    {
        public string AssetId { get; }

        public AssetTotalBalanceSummaryRemovedMessage(string assetId)
        {
            this.AssetId = assetId;
        }
    }
}
