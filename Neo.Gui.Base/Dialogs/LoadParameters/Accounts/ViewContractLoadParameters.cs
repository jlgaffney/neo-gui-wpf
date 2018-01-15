namespace Neo.Gui.Base.Dialogs.LoadParameters.Accounts
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
