using Neo.Wallets;

namespace Neo.UI.Accounts
{
    public partial class ImportCustomContractView
    {
        private readonly ImportCustomContractViewModel viewModel;

        public ImportCustomContractView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as ImportCustomContractViewModel;
        }

        public VerificationContract GetContract()
        {
            return this.viewModel?.GetContract();
        }
    }
}