using System.Windows.Controls;
using Neo.UniversalWallet.WPF.ViewModels;

namespace Neo.UniversalWallet.WPF.Views
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
