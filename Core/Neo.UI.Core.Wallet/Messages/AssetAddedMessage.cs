using Neo.UI.Core.Data;

namespace Neo.UI.Core.Wallet.Messages
{
    public class AssetAddedMessage
    {
        public AssetItem Asset { get; }

        public AssetAddedMessage(AssetItem asset)
        {
            this.Asset = asset;
        }
    }
}
