using System.Windows;
using System.Windows.Controls;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.ViewModels.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class ChangePasswordView : IDialog<ChangePasswordDialogResult>
    {
        private readonly ChangePasswordViewModel viewModel;

        public ChangePasswordView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as ChangePasswordViewModel;
        }

        private void OldPasswordChanged(object sender, RoutedEventArgs e)
        {
            var oldPasswordBox = sender as PasswordBox;

            if (oldPasswordBox == null) return;

            this.viewModel?.UpdateOldPassword(oldPasswordBox.Password);
        }

        private void NewPasswordChanged(object sender, RoutedEventArgs e)
        {
            var newPasswordBox = sender as PasswordBox;

            if (newPasswordBox == null) return;

            this.viewModel?.UpdateNewPassword(newPasswordBox.Password);
        }

        private void ReEnteredNewPasswordChanged(object sender, RoutedEventArgs e)
        {
            var reEnteredNewPasswordBox = sender as PasswordBox;

            if (reEnteredNewPasswordBox == null) return;

            this.viewModel?.UpdateReEnteredNewPassword(reEnteredNewPasswordBox.Password);
        }
    }
}