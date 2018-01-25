namespace Neo.UI.Core.Controllers.TransactionInvokers
{
    public class TestForGasUsageResult
    {
        public string Result { get; private set; }

        public string Fee { get; private set; }

        public bool TransactionExecutionFailed { get; private set; }

        public TestForGasUsageResult(string result, string fee, bool transactionExecutionFailed)
        {
            this.Result = result;
            this.Fee = fee;
            this.TransactionExecutionFailed = transactionExecutionFailed;
        }
    }
}
