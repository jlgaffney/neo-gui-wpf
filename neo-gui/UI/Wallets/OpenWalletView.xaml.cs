using System.IO;
using System.Windows;
using System.Windows.Controls;

using Neo.Properties;
using Neo.UI.ViewModels.Wallets;

namespace Neo.UI.Views.Wallets
{
    /// <summary>
    /// Interaction logic for OpenWalletView.xaml
    /// </summary>
    public partial class OpenWalletView
    {
        private readonly OpenWalletViewModel viewModel;

        public OpenWalletView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as OpenWalletViewModel;

            if (File.Exists(Settings.Default.LastWalletPath) && this.viewModel != null)
            {
                this.viewModel.WalletPath = Settings.Default.LastWalletPath;

                // focus in password input if wallet has been set
                this.Password.Focus();
            }
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            if (passwordBox == null) return;

            this.viewModel?.UpdatePassword(passwordBox.Password);
        }
    }
}