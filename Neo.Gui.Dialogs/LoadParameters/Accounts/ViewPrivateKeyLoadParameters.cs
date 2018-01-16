namespace Neo.Gui.Dialogs.LoadParameters.Accounts
{
    public class ViewPrivateKeyLoadParameters
    {
        public string ScriptHash { get; }
        
        public ViewPrivateKeyLoadParameters(string scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
