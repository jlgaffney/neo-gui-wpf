using System.Windows;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Wallets;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.ViewModels.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    /// <summary>
    /// Interaction logic for OpenWalletView.xaml
    /// </summary>
    public partial class OpenWalletView : IDialog<OpenWalletLoadParameters>
    {
        private readonly OpenWalletViewModel viewModel;

        public OpenWalletView(
            IFileManager fileManager,
            ISettingsManager settingsManager)
        {
            InitializeComponent();

            if (fileManager.FileExists(settingsManager.LastWalletPath))
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