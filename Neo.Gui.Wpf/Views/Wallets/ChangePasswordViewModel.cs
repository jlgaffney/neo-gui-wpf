using System.Windows;
using System.Windows.Input;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly IWalletController walletController;

        private string oldPassword;
        private string newPassword;
        private string reEnteredNewPassword;

        public bool ChangePasswordEnabled =>
            !string.IsNullOrEmpty(this.oldPassword) &&
            !string.IsNullOrEmpty(this.newPassword) &&
                this.newPassword == this.reEnteredNewPassword;

        public ICommand ChangePasswordCommand => new RelayCommand(this.ChangePassword);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public ChangePasswordViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;
        }

        public void UpdateOldPassword(string updatedPassword)
        {
            this.oldPassword = updatedPassword;

            // Update dependent property
            NotifyPropertyChanged(nameof(this.ChangePasswordEnabled));
        }

        public void UpdateNewPassword(string updatedPassword)
        {
            this.newPassword = updatedPassword;

            // Update dependent property
            NotifyPropertyChanged(nameof(this.ChangePasswordEnabled));
        }

        public void UpdateReEnteredNewPassword(string updatedPassword)
        {
            this.reEnteredNewPassword = updatedPassword;

            // Update dependent property
            NotifyPropertyChanged(nameof(this.ChangePasswordEnabled));
        }

        private void ChangePassword()
        {
            // Check new password is not the same as old password
            if (this.oldPassword == this.newPassword)
            {
                MessageBox.Show("New password must be different to old password!");
                return;
            }

            var changedSuccessfully = this.walletController.ChangePassword(this.oldPassword, this.newPassword);

            if (changedSuccessfully)
            {
                MessageBox.Show(Strings.ChangePasswordSuccessful);

                this.TryClose();
            }
            else
            {
                MessageBox.Show(Strings.PasswordIncorrect);
            }
        }

        private void Cancel()
        {
            this.TryClose();
        }
    }
}