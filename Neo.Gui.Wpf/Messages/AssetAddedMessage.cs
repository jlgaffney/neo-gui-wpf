using Neo.Gui.Base.Data;

namespace Neo.Gui.Wpf.Messages
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
