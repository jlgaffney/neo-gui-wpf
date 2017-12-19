using Neo.Gui.Base.Data;

namespace Neo.Gui.Base.Dialogs.Results.Transactions
{
    public class PayToDialogResult
    {
        public PayToDialogResult(TransactionOutputItem output)
        {
            this.Output = output;
        }

        public TransactionOutputItem Output { get; }
    }
}
