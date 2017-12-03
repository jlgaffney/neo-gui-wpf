using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;

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