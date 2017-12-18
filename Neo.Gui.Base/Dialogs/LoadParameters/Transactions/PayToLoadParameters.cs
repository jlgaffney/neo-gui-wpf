using Neo.Wallets;

namespace Neo.Gui.Base.Dialogs.LoadParameters.Transactions
{
    public class PayToLoadParameters
    {
        public PayToLoadParameters(AssetDescriptor asset = null, UInt160 scriptHash = null)
        {
            this.Asset = asset;
            this.ScriptHash = scriptHash;
        }

        public AssetDescriptor Asset { get; }

        public UInt160 ScriptHash { get; }
    }
}
