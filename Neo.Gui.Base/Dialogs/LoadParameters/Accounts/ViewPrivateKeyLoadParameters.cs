using Neo.Wallets;

namespace Neo.Gui.Base.Dialogs.LoadParameters.Accounts
{
    public class ViewPrivateKeyLoadParameters
    {
        public KeyPair Key { get; }

        public UInt160 ScriptHash { get; }

        public ViewPrivateKeyLoadParameters(KeyPair key, UInt160 scriptHash)
        {
            this.Key = key;
            this.ScriptHash = scriptHash;
        }
    }
}
