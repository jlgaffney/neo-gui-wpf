using Microsoft.Win32;
using System.Windows.Input;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;

namespace Neo.UI.Wallets
{
    public class CreateWalletViewModel : ViewModelBase
    {
        private string walletPath;
        private string password;
        private string reEnteredPassword;

        private bool confirmed;

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

        public ICommand GetWalletPathCommand => new RelayCommand(this.GetWalletPath);

        public ICommand ConfirmCommand => new RelayCommand(this.Confirm);

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

        public bool GetWalletOpenInfo(out string path, out string walletPassword)
        {
            path = null;
            walletPassword = null;

            if (!this.confirmed) return false;

            path = this.walletPath;
            walletPassword = this.password;

            return true;
        }

        private void GetWalletPath()
        {
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
            this.confirmed = true;

            this.TryClose();
        }
    }
}