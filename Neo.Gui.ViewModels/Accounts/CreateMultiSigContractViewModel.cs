using System;
using System.Collections.ObjectModel;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.ViewModels.Accounts
{
    public class CreateMultiSigContractViewModel : ViewModelBase, IDialogViewModel<CreateMultiSigContractLoadParameters>
    {
        #region Private Fields 
        private readonly INotificationService notificationService;
        private readonly IWalletController walletController;

        private int minimumSignatureNumber;
        private int minimumSignatureNumberMaxValue;

        private string selectedPublicKey;

        private string newPublicKey;
        #endregion

        #region Public Properties 
        public int MinimumSignatureNumber
        {
            get => this.minimumSignatureNumber;
            set
            {
                if (this.minimumSignatureNumber == value) return;

                this.minimumSignatureNumber = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public int MinimumSignatureNumberMaxValue
        {
            get => this.minimumSignatureNumberMaxValue;
            set
            {
                if (this.minimumSignatureNumberMaxValue == value) return;

                this.minimumSignatureNumberMaxValue = value;

                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> PublicKeys { get; }

        public string SelectedPublicKey
        {
            get => this.selectedPublicKey;
            set
            {
                if (this.selectedPublicKey == value) return;

                this.selectedPublicKey = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.RemovePublicKeyEnabled));
            }
        }

        public string NewPublicKey
        {
            get => this.newPublicKey;
            set
            {
                if (this.newPublicKey == value) return;

                this.newPublicKey = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.AddPublicKeyEnabled));
            }
        }

        public bool AddPublicKeyEnabled => !string.IsNullOrEmpty(this.NewPublicKey);

        public bool RemovePublicKeyEnabled => this.SelectedPublicKey != null;

        public bool ConfirmEnabled => this.MinimumSignatureNumber > 0;

        public RelayCommand AddPublicKeyCommand => new RelayCommand(this.AddPublicKey);

        public RelayCommand RemovePublicKeyCommand => new RelayCommand(this.RemovePublicKey);

        public RelayCommand ConfirmCommand => new RelayCommand(this.Confirm);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public CreateMultiSigContractViewModel(
            INotificationService notificationService,
            IWalletController walletController)
        {
            this.notificationService = notificationService;
            this.walletController = walletController;

            this.PublicKeys = new ObservableCollection<string>();
        }
        #endregion

        #region DialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(CreateMultiSigContractLoadParameters parameters)
        {
        }
        #endregion

        #region Private Methods 
        private void Confirm()
        {
            this.walletController.AddMultiSignatureContract(this.minimumSignatureNumber, this.PublicKeys);

            this.Close(this, EventArgs.Empty);
        }

        private void AddPublicKey()
        {
            if (!this.AddPublicKeyEnabled) return;

            // Check if public key has already been added
            if (this.PublicKeys.Any(publicKey => publicKey.Equals(this.NewPublicKey, StringComparison.InvariantCultureIgnoreCase)))
            {
                this.notificationService.ShowInformationNotification("Public key has already been added.");     // TODO - Issue #130: Add this string to Resources
                return;
            }

            this.PublicKeys.Add(this.NewPublicKey);

            this.NewPublicKey = string.Empty;
            this.MinimumSignatureNumberMaxValue = this.PublicKeys.Count;
        }

        private void RemovePublicKey()
        {
            if (!this.RemovePublicKeyEnabled) return;

            this.PublicKeys.Remove(this.SelectedPublicKey);
            this.MinimumSignatureNumberMaxValue = this.PublicKeys.Count;
        }
        #endregion
    }
}