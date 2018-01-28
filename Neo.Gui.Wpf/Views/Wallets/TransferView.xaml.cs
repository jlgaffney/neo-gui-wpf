using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class TransferView : IDialog<TransferLoadParameters>
    {
        public TransferView()
        {
            InitializeComponent();
        }
    }
}