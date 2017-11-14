namespace Neo.UI.Messages
{
    public class AddAssetMessage
    {
        public AssetItem AssetItem { get; private set; }

        public AddAssetMessage(AssetItem assetItem)
        {
            this.AssetItem = assetItem;
        }
    }
}
