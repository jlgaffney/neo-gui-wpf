using Neo.Wallets;

namespace Neo.Gui.Base.Dialogs.LoadParameters.Accounts
{
    public class ViewPrivateKeyLoadParameters
    {
        public UInt160 ScriptHash { get; }

        public KeyPair Key { get; }
        
        public ViewPrivateKeyLoadParameters(UInt160 scriptHash, KeyPair key)
        {
            this.ScriptHash = scriptHash;
            this.Key = key;
        }
    }
}
