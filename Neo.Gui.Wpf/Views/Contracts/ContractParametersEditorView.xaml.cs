using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Contracts;

namespace Neo.Gui.Wpf.Views.Contracts
{
    /// <summary>
    /// Interaction logic for ContractParametersEditorView.xaml
    /// </summary>
    public partial class ContractParametersEditorView : IDialog<ContractParametersEditorLoadParameters>
    {
        public ContractParametersEditorView()
        {
            InitializeComponent();
        }
    }
}