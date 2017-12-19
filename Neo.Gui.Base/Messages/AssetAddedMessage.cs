using Neo.Gui.Base.Data;

namespace Neo.Gui.Base.Messages
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
