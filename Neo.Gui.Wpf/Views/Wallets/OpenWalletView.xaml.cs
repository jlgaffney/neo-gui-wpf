using System.IO;
using System.Windows;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Managers;

namespace Neo.Gui.Wpf.Views.Wallets
{
    /// <summary>
    /// Interaction logic for OpenWalletView.xaml
    /// </summary>
    public partial class OpenWalletView : IDialog<OpenWalletDialogResult>
    {
        private readonly OpenWalletViewModel viewModel;

        public OpenWalletView(
            ISettingsManager settingsManager)
        {
            InitializeComponent();

            if (File.Exists(settingsManager.LastWalletPath))
            {
                // Focus in password input if wallet path has been set
                this.Password.Focus();
            }

            this.viewModel = this.DataContext as OpenWalletViewModel;
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.viewModel?.UpdatePassword(this.Password.Password);
        }
    }
}