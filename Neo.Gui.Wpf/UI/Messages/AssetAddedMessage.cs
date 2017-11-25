namespace Neo.UI.Messages
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
