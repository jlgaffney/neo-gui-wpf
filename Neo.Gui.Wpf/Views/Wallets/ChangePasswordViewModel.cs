using System;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public class ChangePasswordViewModel : ViewModelBase, IDialogViewModel<ChangePasswordDialogResult>
    {
        private readonly INotificationHelper notificationHelper;
        private readonly IWalletController walletController;

        private string oldPassword;
        private string newPassword;
        private string reEnteredNewPassword;

        public ChangePasswordViewModel(
            INotificationHelper notificationHelper,
            IWalletController walletController)
        {
            this.notificationHelper = notificationHelper;
            this.walletController = walletController;
        }

        public bool ChangePasswordEnabled =>
            !string.IsNullOrEmpty(this.oldPassword) &&
            !string.IsNullOrEmpty(this.newPassword) &&
            this.newPassword == this.reEnteredNewPassword;

        public RelayCommand ChangePasswordCommand => new RelayCommand(this.ChangePassword);

        public RelayCommand CancelCommand => new RelayCommand(this.Cancel);

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<ChangePasswordDialogResult> SetDialogResultAndClose;

        public ChangePasswordDialogResult DialogResult { get; private set; }
        #endregion

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
                this.notificationHelper.ShowWarningNotification("New password must be different to old password!");
                return;
            }

            var changedSuccessfully = this.walletController.ChangePassword(this.oldPassword, this.newPassword);

            if (changedSuccessfully)
            {
                this.notificationHelper.ShowSuccessNotification(Strings.ChangePasswordSuccessful);

                this.Close(this, EventArgs.Empty);
            }
            else
            {
                this.notificationHelper.ShowErrorNotification(Strings.PasswordIncorrect);
            }
        }

        private void Cancel()
        {
            this.Close(this, EventArgs.Empty);
        }
    }
}