using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Neo.Controllers;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Properties;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;
using Neo.Wallets;

namespace Neo.UI.Accounts
{
    public class CreateMultiSigContractViewModel : ViewModelBase
    {
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;
        private readonly IDispatcher dispatcher;

        private int minimumSignatureNumber;
        private int minimumSignatureNumberMaxValue;

        private string selectedPublicKey;

        private string newPublicKey;


        public CreateMultiSigContractViewModel(
            IWalletController walletController,
            IMessagePublisher messagePublisher,
            IDispatcher dispatcher)
        {
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;
            this.dispatcher = dispatcher;

            this.PublicKeys = new ObservableCollection<string>();
        }

        public int MinimumSignatureNumber
        {
            get => this.minimumSignatureNumber;
            set
            {
                if (this.minimumSignatureNumber == value) return;

                this.minimumSignatureNumber = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public int MinimumSignatureNumberMaxValue
        {
            get => this.minimumSignatureNumberMaxValue;
            set
            {
                if (this.minimumSignatureNumberMaxValue == value) return;

                this.minimumSignatureNumberMaxValue = value;

                NotifyPropertyChanged();
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

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.RemovePublicKeyEnabled));
            }
        }

        public string NewPublicKey
        {
            get => this.newPublicKey;
            set
            {
                if (this.newPublicKey == value) return;

                this.newPublicKey = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.AddPublicKeyEnabled));
            }
        }

        public bool AddPublicKeyEnabled => !string.IsNullOrEmpty(this.NewPublicKey);

        public bool RemovePublicKeyEnabled => this.SelectedPublicKey != null;


        public bool ConfirmEnabled => this.MinimumSignatureNumber > 0;

        public ICommand AddPublicKeyCommand => new RelayCommand(this.AddPublicKey);

        public ICommand RemovePublicKeyCommand => new RelayCommand(this.RemovePublicKey);

        public ICommand ConfirmCommand => new RelayCommand(this.Confirm);

        public ICommand CancelCommand => new RelayCommand(this.TryClose);


        private void Confirm()
        {
            var contract = this.GenerateContract();

            if (contract == null)
            {
                MessageBox.Show(Strings.AddContractFailedMessage);
                return;
            }

            this.messagePublisher.Publish(new AddContractMessage(contract));
            this.TryClose();
        }

        private async void AddPublicKey()
        {
            if (!this.AddPublicKeyEnabled) return;

            // Check if public key has already been added
            if (this.PublicKeys.Any(publicKey => publicKey.Equals(this.NewPublicKey, StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageBox.Show("Public key has already been added.");
                return;
            }

            await this.dispatcher.InvokeOnMainUIThread(() => this.PublicKeys.Add(this.NewPublicKey));

            this.NewPublicKey = string.Empty;
            this.MinimumSignatureNumberMaxValue = this.PublicKeys.Count;
        }

        private async void RemovePublicKey()
        {
            if (!this.RemovePublicKeyEnabled) return;

            await this.dispatcher.InvokeOnMainUIThread(() =>
            {
                this.PublicKeys.Remove(this.SelectedPublicKey);
                this.MinimumSignatureNumberMaxValue = this.PublicKeys.Count;
            });
        }

        private VerificationContract GenerateContract()
        {
            var publicKeys = this.PublicKeys.Select(p => ECPoint.DecodePoint(p.HexToBytes(), ECCurve.Secp256r1)).ToArray();

            foreach (var publicKey in publicKeys)
            {
                var key = this.walletController.GetKey(publicKey.EncodePoint(true).ToScriptHash());

                if (key == null) continue;

                return VerificationContract.CreateMultiSigContract(key.PublicKeyHash, this.MinimumSignatureNumber, publicKeys);
            }

            return null;
        }
    }
}