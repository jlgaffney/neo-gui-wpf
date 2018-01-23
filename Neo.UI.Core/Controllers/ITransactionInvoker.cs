namespace Neo.UI.Core.Controllers
{
    public interface ITransactionInvoker
    {
        string GetTransactionScript();

        string TestForGasUsage();

        void Invoke();
    }
}
