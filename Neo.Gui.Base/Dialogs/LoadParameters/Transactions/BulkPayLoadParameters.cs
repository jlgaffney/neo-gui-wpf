using Neo.Wallets;

namespace Neo.Gui.Base.Dialogs.LoadParameters.Transactions
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
