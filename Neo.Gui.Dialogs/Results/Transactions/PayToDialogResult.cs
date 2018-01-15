using Neo.UI.Core.Data;

namespace Neo.Gui.Dialogs.Results.Transactions
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
