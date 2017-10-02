using System.Windows;
using System.Windows.Input;
using Neo.Properties;
using Neo.UI.Controls;
using Neo.UI.Messages;
using Neo.UI.MVVM;

namespace Neo.UI.ViewModels.Dialogs
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private NeoWindow view;

        private string oldPassword;
        private string newPassword;
        private string reEnteredNewPassword;

        public bool ChangePasswordEnabled =>
            !string.IsNullOrEmpty(this.oldPassword) &&
            !string.IsNullOrEmpty(this.newPassword) &&
                this.newPassword == this.reEnteredNewPassword;

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public override void OnViewAttached(object view)
        {
            this.view = view as NeoWindow;
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

        private void Ok()
        {
            // Check new password is not the same as old password
            if (this.oldPassword == this.newPassword)
            {
                MessageBox.Show("New password must be different to old password!");
                return;
            }

            var changedSuccessfully = App.CurrentWallet.ChangePassword(this.oldPassword, this.newPassword);

            if (changedSuccessfully)
            {
                MessageBox.Show(Strings.ChangePasswordSuccessful);

                this.view?.Close();
            }
            else
            {
                MessageBox.Show(Strings.PasswordIncorrect);
            }
        }

        private void Cancel()
        {
            this.view?.Close();
        }
    }
}