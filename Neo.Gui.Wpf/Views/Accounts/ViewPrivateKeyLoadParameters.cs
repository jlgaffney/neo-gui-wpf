using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public class ViewPrivateKeyLoadParameters
    {
        public KeyPair Key { get; private set; }

        public UInt160 ScriptHash { get; private set; }

        public ViewPrivateKeyLoadParameters(KeyPair key, UInt160 scriptHash)
        {
            this.Key = key;
            this.ScriptHash = scriptHash;
        }
    }
}
