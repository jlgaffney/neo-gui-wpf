using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Contracts;

namespace Neo.Gui.Wpf.Views.Contracts
{
    /// <summary>
    /// Interaction logic for ContractParametersEditorView.xaml
    /// </summary>
    public partial class ContractParametersEditorView : IDialog<ContractParametersEditorDialogResult>
    {
        public ContractParametersEditorView()
        {
            InitializeComponent();
        }
    }
}