using Neo.UI.ViewModels.Accounts;
using Neo.Wallets;

namespace Neo.UI.Views.Accounts
{
    public partial class ViewPrivateKeyView
    {
        public ViewPrivateKeyView(KeyPair key, UInt160 scriptHash)
        {
            InitializeComponent();

            var viewModel = this.DataContext as ViewPrivateKeyViewModel;

            viewModel?.SetKeyInfo(key, scriptHash);
        }
    }
}