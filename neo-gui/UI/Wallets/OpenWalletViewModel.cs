using System.Windows.Input;
using Microsoft.Win32;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;

namespace Neo.UI.Wallets
{
    public class OpenWalletViewModel : ViewModelBase
    {
        private string walletPath;
        private string password;
        private bool repairMode;

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

        public bool RepairMode
        {
            get { return this.repairMode; }
            set
            {
                if (this.repairMode == value) return;

                this.repairMode = value;

                NotifyPropertyChanged();
            }
        }

        public bool ConfirmEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(this.WalletPath) || string.IsNullOrEmpty(this.password))
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

        private void GetWalletPath()
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = "db3",
                Filter = "Wallet File|*.db3"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                this.WalletPath = openFileDialog.FileName;
            }
        }

        private void Confirm()
        {
            EventAggregator.Current.Publish(new OpenWalletMessage(this.WalletPath, this.password, this.RepairMode));

            this.TryClose();
        }
    }
}