namespace Neo.UI.Core.Transactions.Testing
{
    public class TestForGasUsageResult
    {
        public string Result { get; }

        public string Fee { get; }

        public bool TransactionExecutionFailed { get; }

        public TestForGasUsageResult(string result, string fee, bool transactionExecutionFailed)
        {
            this.Result = result;
            this.Fee = fee;
            this.TransactionExecutionFailed = transactionExecutionFailed;
        }
    }
}
