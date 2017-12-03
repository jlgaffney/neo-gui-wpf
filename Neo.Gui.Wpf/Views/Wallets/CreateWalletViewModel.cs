using System;
using Microsoft.Win32;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public class CreateWalletViewModel : ViewModelBase, IDialogViewModel<CreateWalletDialogResult>
    {
        private string walletPath;
        private string password;
        private string reEnteredPassword;
        
        public string WalletPath
        {
            get => this.walletPath;
            set
            {
                if (this.walletPath == value) return;

                this.walletPath = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public bool ConfirmEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(this.WalletPath) || string.IsNullOrEmpty(this.password) || string.IsNullOrEmpty(this.reEnteredPassword))
                {
                    return false;
                }

                // Check user re-entered password
                if (this.password != this.reEnteredPassword)
                {
                    return false;
                }

                return true;
            }
        }

        public RelayCommand GetWalletPathCommand => new RelayCommand(this.GetWalletPath);

        public RelayCommand ConfirmCommand => new RelayCommand(this.Confirm);

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<CreateWalletDialogResult> SetDialogResultAndClose;

        public CreateWalletDialogResult DialogResult { get; private set; }
        #endregion

        public void UpdatePassword(string updatedPassword)
        {
            this.password = updatedPassword;

            // Update dependent property
            NotifyPropertyChanged(nameof(this.ConfirmEnabled));
        }

        public void UpdateReEnteredPassword(string updatedReEnteredPassword)
        {
            this.reEnteredPassword = updatedReEnteredPassword;

            // Update dependent property
            NotifyPropertyChanged(nameof(this.ConfirmEnabled));
        }

        private void GetWalletPath()
        {
            // TODO Issue #76 [AboimPinto]: SaveDialog need to be abstracted
            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = "db3",
                Filter = "Wallet File|*.db3"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                this.WalletPath = saveFileDialog.FileName;
            }
        }

        private void Confirm()
        {
            if (!this.ConfirmEnabled) return;

            if (this.SetDialogResultAndClose != null)
            {
                var dialogResult = new CreateWalletDialogResult(
                    this.WalletPath,
                    this.password);
                this.SetDialogResultAndClose(this, dialogResult);
            }

            this.Close(this, EventArgs.Empty);
        }
    }
}