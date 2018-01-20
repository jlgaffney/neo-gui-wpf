namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class DeployContractParameters
    {
        public string ContractSourceCode { get; private set; }

        public DeployContractParameters(string contactSourceCode)
        {
            this.ContractSourceCode = ContractSourceCode;
        }
    }
}
