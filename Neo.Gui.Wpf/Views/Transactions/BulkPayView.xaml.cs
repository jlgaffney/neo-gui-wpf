using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Transactions;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public partial class BulkPayView : IDialog<BulkPayDialogResult>
    {
        public BulkPayView()
        {
            InitializeComponent();
        }
    }
}