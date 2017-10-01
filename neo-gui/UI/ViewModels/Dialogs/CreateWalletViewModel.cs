using Microsoft.Win32;
using System.Windows.Input;

using Neo.UI.Controls;
using Neo.UI.Messages;
using Neo.UI.MVVM;

namespace Neo.UI.ViewModels.Dialogs
{
    public class CreateWalletViewModel : ViewModelBase
    {
        private NeoWindow view;

        private string walletPath;
        private string password;
        private string reEnteredPassword;

        public string WalletPath
        {
            get { return this.walletPath; }
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

        public override void OnViewAttached(NeoWindow attachedView)
        {
            this.view = attachedView;
        }

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
            EventAggregator.Current.Publish(new CreateWalletMessage(this.WalletPath, this.password));

            this.view?.Close();
        }
    }
}