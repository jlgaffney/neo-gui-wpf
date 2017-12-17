using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Transactions;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public partial class SigningView : IDialog<SigningDialogResult>
    {
        public SigningView()
        {
            InitializeComponent();
        }
    }
}