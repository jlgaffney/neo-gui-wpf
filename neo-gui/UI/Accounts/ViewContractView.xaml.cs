using Neo.Wallets;

namespace Neo.UI.Accounts
{
    public partial class ViewContractView
    {
        public ViewContractView(VerificationContract contract)
        {
            InitializeComponent();

            var viewModel = this.DataContext as ViewContractViewModel;

            viewModel?.SetContract(contract);
        }
    }
}