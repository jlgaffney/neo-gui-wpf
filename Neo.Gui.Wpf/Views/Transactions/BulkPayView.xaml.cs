using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Transactions;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public partial class BulkPayView : IDialog<BulkPayLoadParameters>
    {
        public BulkPayView()
        {
            InitializeComponent();
        }
    }
}