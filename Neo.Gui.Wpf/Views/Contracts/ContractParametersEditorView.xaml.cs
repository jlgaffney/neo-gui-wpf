using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Contracts;

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