namespace Neo.Gui.Base.Dialogs.LoadParameters.Accounts
{
    public class ViewPrivateKeyLoadParameters
    {
        public UInt160 ScriptHash { get; }
        
        public ViewPrivateKeyLoadParameters(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
