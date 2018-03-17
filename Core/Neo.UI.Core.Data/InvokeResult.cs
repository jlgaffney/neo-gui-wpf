namespace Neo.UI.Core.Data
{
    public class InvokeResult
    {
        public bool ExecutionSucceeded { get; }

        public string Result { get; }

        public decimal Fee { get; }

        public InvokeResult(bool executionSucceeded, string result, decimal fee)
        {
            this.ExecutionSucceeded = executionSucceeded;
            this.Result = result;
            this.Fee = fee;
        }
    }
}
