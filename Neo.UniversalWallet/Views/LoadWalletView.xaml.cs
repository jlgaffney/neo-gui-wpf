using Neo.UniversalWallet.ViewModels;
using System.Windows.Controls;

namespace Neo.UniversalWallet.Views
{
    public partial class LoadWalletView : Page
    {
        public LoadWalletView()
        {
            this.DataContext = new LoadWalletViewModel();

            InitializeComponent();
        }
    }
}
