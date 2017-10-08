using System.Collections.Generic;
using Neo.SmartContract;

namespace Neo.UI.Contracts
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