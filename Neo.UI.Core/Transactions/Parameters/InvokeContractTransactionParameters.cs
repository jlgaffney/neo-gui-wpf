namespace Neo.UI.Core.Transactions.Parameters
{
    public class InvokeContractTransactionParameters : TransactionParameters
    {
        public byte[] Script { get; }

        public InvokeContractTransactionParameters(byte[] script)
        {
            this.Script = script;
        }
    }
}
