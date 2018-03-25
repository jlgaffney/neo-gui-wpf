namespace Neo.UI.Core.Wallet.Messages
{
    public class AssetIssuerInfoUpdatedMessage
    {
        public string AssetId { get; }

        public string Issuer { get; }

        public AssetIssuerInfoUpdatedMessage(string assetId, string issuer)
        {
            this.AssetId = assetId;
            this.Issuer = issuer;
        }
    }
}
