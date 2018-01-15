using Neo.UI.Core.Data;

namespace Neo.Gui.Dialogs.Results.Transactions
{
    public class BulkPayDialogResult
    {
        public BulkPayDialogResult(TransactionOutputItem[] outputs)
        {
            this.Outputs = outputs;
        }

        public TransactionOutputItem[] Outputs { get; }
    }
}
