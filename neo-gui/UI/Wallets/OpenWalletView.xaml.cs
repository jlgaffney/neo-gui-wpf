using System.IO;
using System.Windows;
using System.Windows.Controls;
using Neo.Properties;

namespace Neo.UI.Wallets
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

        public bool GetWalletOpenInfo(out string walletPath, out string password, out bool repairMode)
        {
            walletPath = null;
            password = null;
            repairMode = false;

            if (this.viewModel == null) return false;

            return this.viewModel.GetWalletOpenInfo(out walletPath, out password, out repairMode);
        }
    }
}