using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public partial class CreateMultiSigContractView : IDialog<CreateMultiSigContractDialogResult>
    {
        public CreateMultiSigContractView()
        {
            InitializeComponent();
        }
    }
}