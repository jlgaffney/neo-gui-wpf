namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class InvokeContractLoadParameters
    {
        public byte[] Script { get; }

        public InvokeContractLoadParameters(byte[] script)
        {
            this.Script = script;
        }
    }
}

