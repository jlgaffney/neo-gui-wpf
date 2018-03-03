using Neo.UI.Core.Data.Enums;

namespace Neo.UI.Core.Wallet.Messages
{
    public class AssetAddedMessage
    {
        public string AssetId { get; }

        public string AssetName { get; }

        public string AssetIssuer { get; }

        public string Type { get; }

        public TokenType TokenType { get; }

        public bool IsSystemAsset { get; }

        public string TotalBalance { get; }

        public AssetAddedMessage(string assetId, string assetName, string assetIssuer, string type, TokenType tokenType, bool isSystemAsset, string totalBalance)
        {
            this.AssetId = assetId;
            this.AssetName = assetName;
            this.AssetIssuer = assetIssuer;
            this.Type = type;
            this.TokenType = tokenType;
            this.IsSystemAsset = isSystemAsset;

            this.TotalBalance = totalBalance;
        }
    }
}
