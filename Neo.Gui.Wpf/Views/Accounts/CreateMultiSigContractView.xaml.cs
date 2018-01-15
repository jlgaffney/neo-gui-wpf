using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public partial class CreateMultiSigContractView : IDialog<CreateMultiSigContractLoadParameters>
    {
        public CreateMultiSigContractView()
        {
            InitializeComponent();
        }
    }
}