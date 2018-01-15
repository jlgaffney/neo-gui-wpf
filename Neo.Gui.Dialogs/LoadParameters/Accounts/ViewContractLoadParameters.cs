namespace Neo.Gui.Dialogs.LoadParameters.Accounts
{
    public class ViewContractLoadParameters
    {
        public string ScriptHash { get; }

        public ViewContractLoadParameters(string scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
