using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Cryptography.ECC;
using Neo.SmartContract;

using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Services;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Managers;

namespace Neo.Gui.ViewModels.Accounts
{
    public class CreateMultiSigContractViewModel : ViewModelBase, IDialogViewModel<CreateMultiSigContractDialogResult>
    {
        private readonly IDialogManager dialogManager;
        private readonly IMessagePublisher messagePublisher;
        private readonly IDispatchService dispatchService;

        private int minimumSignatureNumber;
        private int minimumSignatureNumberMaxValue;

        private string selectedPublicKey;

        private string newPublicKey;

        public CreateMultiSigContractViewModel(
            IDialogManager dialogManager,
            IMessagePublisher messagePublisher,
            IDispatchService dispatchService)
        {
            this.dialogManager = dialogManager;
            this.messagePublisher = messagePublisher;
            this.dispatchService = dispatchService;

            this.PublicKeys = new ObservableCollection<string>();
        }

        #region DialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<CreateMultiSigContractDialogResult> SetDialogResultAndClose;

        public CreateMultiSigContractDialogResult DialogResult { get; private set; }
        #endregion

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

        public ICommand AddPublicKeyCommand => new RelayCommand(this.AddPublicKey);

        public ICommand RemovePublicKeyCommand => new RelayCommand(this.RemovePublicKey);

        public ICommand ConfirmCommand => new RelayCommand(this.Confirm);

        public ICommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        private void Confirm()
        {
            var contract = this.GenerateContract();

            if (contract == null)
            {
                this.dialogManager.ShowMessageDialog(string.Empty, Strings.AddContractFailedMessage);
                return;
            }

            this.messagePublisher.Publish(new AddContractMessage(contract));

            this.Close(this, EventArgs.Empty);
        }

        private async void AddPublicKey()
        {
            if (!this.AddPublicKeyEnabled) return;

            // Check if public key has already been added
            if (this.PublicKeys.Any(publicKey => publicKey.Equals(this.NewPublicKey, StringComparison.InvariantCultureIgnoreCase)))
            {
                this.dialogManager.ShowMessageDialog(string.Empty, "Public key has already been added.");
                return;
            }

            await this.dispatchService.InvokeOnMainUIThread(() => this.PublicKeys.Add(this.NewPublicKey));

            this.NewPublicKey = string.Empty;
            this.MinimumSignatureNumberMaxValue = this.PublicKeys.Count;
        }

        private async void RemovePublicKey()
        {
            if (!this.RemovePublicKeyEnabled) return;

            await this.dispatchService.InvokeOnMainUIThread(() =>
            {
                this.PublicKeys.Remove(this.SelectedPublicKey);
                this.MinimumSignatureNumberMaxValue = this.PublicKeys.Count;
            });
        }

        private Contract GenerateContract()
        {
            var publicKeys = this.PublicKeys.Select(p => ECPoint.DecodePoint(p.HexToBytes(), ECCurve.Secp256r1)).ToArray();

            return Contract.CreateMultiSigContract(this.MinimumSignatureNumber, publicKeys);
        }
    }
}