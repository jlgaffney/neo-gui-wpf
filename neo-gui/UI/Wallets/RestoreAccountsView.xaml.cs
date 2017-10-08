using System.Collections.Generic;
using Neo.Wallets;

namespace Neo.UI.Wallets
{
    public partial class RestoreAccountsView
    {
        private readonly RestoreAccountsViewModel viewModel;

        public RestoreAccountsView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as RestoreAccountsViewModel;
        }

        public List<VerificationContract> GetContracts()
        {
            return this.viewModel?.GetContracts();
        }
    }
}