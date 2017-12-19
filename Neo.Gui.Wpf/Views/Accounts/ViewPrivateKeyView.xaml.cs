using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public partial class ViewPrivateKeyView : IDialog<ViewPrivateKeyDialogResult>
    {
        public ViewPrivateKeyView()
        {
            InitializeComponent();
        }
    }
}