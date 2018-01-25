using Neo.UI.Core.Data;

namespace Neo.Gui.Dialogs.LoadParameters.Transactions
{
    public class BulkPayLoadParameters
    {
        public AssetDto Asset { get; }

        public BulkPayLoadParameters(AssetDto asset = null)
        {
            this.Asset = asset;
        }
    }
}
