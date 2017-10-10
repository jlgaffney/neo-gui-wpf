using Neo.Wallets;

namespace Neo.UI.Accounts
{
    public partial class CreateLockAccountView
    {
        private readonly CreateLockAccountViewModel viewModel;

        public CreateLockAccountView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as CreateLockAccountViewModel;
        }

        public VerificationContract GetContract()
        {
            return this.viewModel?.GetContract();
        }
    }
}