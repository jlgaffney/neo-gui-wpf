using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Properties;
using Neo.UI.Base.MVVM;
using Neo.Wallets;

namespace Neo.UI.Accounts
{
    public class CreateMultiSigContractViewModel : ViewModelBase
    {
        private int minimumSignatureNumber;
        private int minimumSignatureNumberMaxValue;

        private string selectedPublicKey;

        private string newPublicKey;


        private VerificationContract contract;

        public CreateMultiSigContractViewModel()
        {
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
            this.contract = this.GenerateContract();

            if (this.contract == null)
            {
                MessageBox.Show(Strings.AddContractFailedMessage);
                return;
            }

            this.TryClose();
        }

        private void AddPublicKey()
        {
            if (!this.AddPublicKeyEnabled) return;

            // Check if public key has already been added
            if (this.PublicKeys.Any(publicKey => publicKey.Equals(this.NewPublicKey, StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageBox.Show("Public key has already been added.");
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

        public VerificationContract GetContract()
        {
            return this.contract;
        }

        private VerificationContract GenerateContract()
        {
            var publicKeys = this.PublicKeys.Select(p => ECPoint.DecodePoint(p.HexToBytes(), ECCurve.Secp256r1)).ToArray();

            foreach (var publicKey in publicKeys)
            {
                var key = ApplicationContext.Instance.CurrentWallet.GetKey(publicKey.EncodePoint(true).ToScriptHash());

                if (key == null) continue;

                return VerificationContract.CreateMultiSigContract(key.PublicKeyHash, this.MinimumSignatureNumber, publicKeys);
            }

            return null;
        }
    }
}