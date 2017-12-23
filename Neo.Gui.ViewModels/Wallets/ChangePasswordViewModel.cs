using System;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Globalization.Resources;
using Neo.Gui.Base.Services;

namespace Neo.Gui.ViewModels.Wallets
{
    public class ChangePasswordViewModel : ViewModelBase, IDialogViewModel<ChangePasswordDialogResult>
    {
        private readonly INotificationService notificationService;
        private readonly IWalletController walletController;

        private string oldPassword;
        private string newPassword;
        private string reEnteredNewPassword;

        public ChangePasswordViewModel(
            INotificationService notificationService,
            IWalletController walletController)
        {
            this.notificationService = notificationService;
            this.walletController = walletController;
        }

        public bool ChangePasswordEnabled =>
            !string.IsNullOrEmpty(this.oldPassword) &&
            !string.IsNullOrEmpty(this.newPassword) &&
            this.newPassword == this.reEnteredNewPassword;

        public ICommand ChangePasswordCommand => new RelayCommand(this.ChangePassword);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<ChangePasswordDialogResult> SetDialogResultAndClose;

        public ChangePasswordDialogResult DialogResult { get; private set; }
        #endregion

        public void UpdateOldPassword(string updatedPassword)
        {
            this.oldPassword = updatedPassword;

            // Update dependent property
            RaisePropertyChanged(nameof(this.ChangePasswordEnabled));
        }

        public void UpdateNewPassword(string updatedPassword)
        {
            this.newPassword = updatedPassword;

            // Update dependent property
            RaisePropertyChanged(nameof(this.ChangePasswordEnabled));
        }

        public void UpdateReEnteredNewPassword(string updatedPassword)
        {
            this.reEnteredNewPassword = updatedPassword;

            // Update dependent property
            RaisePropertyChanged(nameof(this.ChangePasswordEnabled));
        }

        private void ChangePassword()
        {
            // Check new password is not the same as old password
            if (this.oldPassword == this.newPassword)
            {
                this.notificationService.ShowWarningNotification("New password must be different to old password!");
                return;
            }

            var changedSuccessfully = this.walletController.ChangePassword(this.oldPassword, this.newPassword);

            if (changedSuccessfully)
            {
                this.notificationService.ShowSuccessNotification(Strings.ChangePasswordSuccessful);

                this.Close(this, EventArgs.Empty);
            }
            else
            {
                this.notificationService.ShowErrorNotification(Strings.PasswordIncorrect);
            }
        }

        private void Cancel()
        {
            this.Close(this, EventArgs.Empty);
        }
    }
}