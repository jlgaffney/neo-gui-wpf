using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Neo.Properties;

namespace Neo.UI.Home
{
    /// <summary>
    /// Interaction logic for AccountsView.xaml
    /// </summary>
    public partial class AccountsView
    {
        private AccountsViewModel viewModel;

        public AccountsView()
        {
            InitializeComponent();
        }

        private void AccountsView_Loaded(object sender, RoutedEventArgs e)
        {
            this.AttachViewModel();
        }

        private void AttachViewModel()
        {
            // Check if view model has already been attached
            if (this.viewModel != null) return;

            this.viewModel = this.DataContext as AccountsViewModel;
        }
        
        private void AccountList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.viewModel?.SelectedAccount == null) return;
            var url = string.Format(Settings.Default.Urls.AddressUrl, this.viewModel?.SelectedAccount.Address);
            Process.Start(url);
        }
    }
}