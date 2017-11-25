using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
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