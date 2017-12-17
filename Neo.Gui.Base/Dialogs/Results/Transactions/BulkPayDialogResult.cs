using Neo.Gui.Base.Data;

namespace Neo.Gui.Base.Dialogs.Results.Transactions
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
