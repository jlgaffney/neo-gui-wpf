using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Transactions;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public partial class PayToView : IDialog<PayToLoadParameters>
    {
        public PayToView()
        {
            InitializeComponent();
        }
    }
}
