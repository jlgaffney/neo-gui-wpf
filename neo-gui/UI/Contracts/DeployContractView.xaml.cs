using Neo.Core;

namespace Neo.UI.Contracts
{
    /// <summary>
    /// Interaction logic for DeployContractView.xaml
    /// </summary>
    public partial class DeployContractView
    {
        private readonly DeployContractViewModel viewModel;

        public DeployContractView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as DeployContractViewModel;
        }

        public InvocationTransaction GetTransaction()
        {
            return this.viewModel?.GetTransaction();
        }
    }
}