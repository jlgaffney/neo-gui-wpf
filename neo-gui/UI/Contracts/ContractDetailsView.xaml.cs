using Neo.Wallets;

namespace Neo.UI.Contracts
{
    /// <summary>
    /// Interaction logic for ContractDetailsView.xaml
    /// </summary>
    public partial class ContractDetailsView
    {
        public ContractDetailsView(VerificationContract contract)
        {
            InitializeComponent();

            var viewModel = this.DataContext as ContractDetailsViewModel;

            viewModel?.SetContract(contract);
        }
    }
}