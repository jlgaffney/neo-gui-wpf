using System.Collections.Generic;
using Neo.SmartContract;

namespace Neo.Gui.Wpf.Views.Contracts
{
    /// <summary>
    /// Interaction logic for ParametersEditorView.xaml
    /// </summary>
    public partial class ParametersEditorView
    {
        public ParametersEditorView(IList<ContractParameter> parameters)
        {
            InitializeComponent();

            var viewModel = this.DataContext as ParametersEditorViewModel;

            viewModel?.Load(parameters);
        }
    }
}