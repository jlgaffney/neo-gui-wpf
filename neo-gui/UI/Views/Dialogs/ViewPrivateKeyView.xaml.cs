using Neo.UI.ViewModels.Dialogs;
using Neo.Wallets;

namespace Neo.UI.Views.Dialogs
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