using System.Diagnostics;
using System.Windows.Input;
using Neo.Properties;

namespace Neo.UI.Home
{
    /// <summary>
    /// Interaction logic for AccountsView.xaml
    /// </summary>
    public partial class AccountsView
    {
        public AccountsView()
        {
            InitializeComponent();
        }
        
        private void AccountList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.DataContext as AccountsViewModel;

            if (viewModel == null) return;

            if (viewModel.SelectedAccount == null) return;

            var url = string.Format(Settings.Default.Urls.AddressUrl, viewModel.SelectedAccount.Address);

            Process.Start(url);
        }
    }
}