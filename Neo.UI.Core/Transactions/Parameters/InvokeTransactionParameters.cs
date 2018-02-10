namespace Neo.UI.Core.Transactions.Parameters
{
    public class InvokeTransactionParameters
    {
        public string ContractSourceCode { get; }

        public InvokeTransactionParameters(string contractSourceCode)
        {
            this.ContractSourceCode = contractSourceCode;
        }
    }
}
