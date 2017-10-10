using Neo.Wallets;

namespace Neo.UI.Accounts
{
    public partial class CreateMultiSigContractView
    {
        private readonly CreateMultiSigContractViewModel viewModel;

        public CreateMultiSigContractView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as CreateMultiSigContractViewModel;
        }

        public VerificationContract GetContract()
        {
            return this.viewModel?.GetContract();
        }
    }
}