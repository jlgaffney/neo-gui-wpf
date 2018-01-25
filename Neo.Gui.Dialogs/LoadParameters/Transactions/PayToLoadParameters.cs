using Neo.UI.Core.Data;

namespace Neo.Gui.Dialogs.LoadParameters.Transactions
{
    public class PayToLoadParameters
    {
        public AssetDto Asset { get; }

        public string ScriptHash { get; }

        public PayToLoadParameters(AssetDto asset, string scriptHash)
        {
            this.Asset = asset;
            this.ScriptHash = scriptHash;
        }
    }
}
