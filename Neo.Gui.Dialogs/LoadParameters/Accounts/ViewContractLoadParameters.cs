namespace Neo.Gui.Dialogs.LoadParameters.Accounts
{
    public class ViewContractLoadParameters
    {
        public UInt160 ScriptHash { get; }

        public ViewContractLoadParameters(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
