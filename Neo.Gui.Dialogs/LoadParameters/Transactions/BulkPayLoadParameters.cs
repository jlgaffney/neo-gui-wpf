using Neo.Wallets;

namespace Neo.Gui.Dialogs.LoadParameters.Transactions
{
    public class BulkPayLoadParameters
    {
        public BulkPayLoadParameters(AssetDescriptor asset = null)
        {
            this.Asset = asset;
        }

        public AssetDescriptor Asset { get; }
    }
}
