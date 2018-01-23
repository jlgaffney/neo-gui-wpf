namespace Neo.UI.Core.Data.TransactionParameters
{
    public class DeployContractTransactionParameters
    {
        public string ContractSourceCode { get; private set; }

        public DeployContractTransactionParameters(string contactSourceCode)
        {
            this.ContractSourceCode = ContractSourceCode;
        }
    }
}
