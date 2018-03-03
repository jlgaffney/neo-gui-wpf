using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Messages
{
    public class AssetAddedMessage
    {
        public AssetSummary Asset { get; }

        public AssetAddedMessage(AssetSummary asset)
        {
            this.Asset = asset;
        }
    }
}
