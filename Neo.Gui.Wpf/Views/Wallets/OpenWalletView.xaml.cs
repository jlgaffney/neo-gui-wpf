using System.IO;
using System.Windows;
using System.Windows.Controls;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using NeoSettings = Neo.Gui.Wpf.Properties.Settings;

namespace Neo.Gui.Wpf.Views.Wallets
{
    /// <summary>
    /// Interaction logic for OpenWalletView.xaml
    /// </summary>
    public partial class OpenWalletView : IDialog<OpenWalletDialogResult>
    {
        private readonly OpenWalletViewModel viewModel;

        public OpenWalletView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as OpenWalletViewModel;

            if (File.Exists(NeoSettings.Default.LastWalletPath) && this.viewModel != null)
            {
                this.viewModel.WalletPath = NeoSettings.Default.LastWalletPath;

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