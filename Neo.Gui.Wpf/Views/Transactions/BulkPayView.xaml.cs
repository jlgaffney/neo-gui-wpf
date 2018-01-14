using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Transactions;

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