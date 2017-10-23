using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CERTENROLLLib;
using Microsoft.Win32;
using Neo.Cryptography.ECC;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Wallets
{
    public class CertificateRequestViewModel : ViewModelBase
    {
        private ECPoint selectedPublicKey;

        private string cn;
        private string c;
        private string s;
        private string serialNumber;

        public CertificateRequestViewModel()
        {
            this.PublicKeys = App.CurrentWallet.GetContracts().Where(p => p.IsStandard).Select(p =>
                App.CurrentWallet.GetKey(p.PublicKeyHash).PublicKey).ToArray();
        }

        public ECPoint[] PublicKeys { get; }

        public ECPoint SelectedPublicKey
        {
            get => this.selectedPublicKey;
            set
            {
                if (Equals(this.selectedPublicKey, value)) return;

                this.selectedPublicKey = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public string CN
        {
            get => this.cn;
            set
            {
                if (this.cn == value) return;

                this.cn = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public string C
        {
            get => this.c;
            set
            {
                if (this.c == value) return;

                this.c = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public string S
        {
            get => this.s;
            set
            {
                if (this.s == value) return;

                this.s = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public string SerialNumber
        {
            get => this.serialNumber;
            set
            {
                if (this.serialNumber == value) return;

                this.serialNumber = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public bool RequestCertificateEnabled => 
            this.SelectedPublicKey != null &&
            !string.IsNullOrEmpty(this.CN) &&
            !string.IsNullOrEmpty(this.C) &&
            !string.IsNullOrEmpty(this.S) &&
            !string.IsNullOrEmpty(this.SerialNumber);

        public ICommand RequestCertificateCommand => new RelayCommand(this.RequestCertificate);

        public ICommand CancelCommand => new RelayCommand(this.TryClose);

        private void RequestCertificate()
        {
            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = "req",
                Filter = "Certificate Request|*.req|All files|*.*"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var point = this.SelectedPublicKey;
            var key = App.CurrentWallet.GetKey(point);
            var publicKey = point.EncodePoint(false).Skip(1).ToArray();

            byte[] privateKey;
            using (key.Decrypt())
            {
                const int ECDSA_PRIVATE_P256_MAGIC = 0x32534345;
                privateKey = BitConverter.GetBytes(ECDSA_PRIVATE_P256_MAGIC).Concat(BitConverter.GetBytes(32)).Concat(publicKey).Concat(key.PrivateKey).ToArray();
            }

            var x509Key = new CX509PrivateKey
            {
                //AlgorithmName = "ECDSA_P256"
            };

            x509Key.Import("ECCPRIVATEBLOB", Convert.ToBase64String(privateKey));

            Array.Clear(privateKey, 0, privateKey.Length);

            var request = new CX509CertificateRequestPkcs10();

            request.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, x509Key, null);
            request.Subject = new CX500DistinguishedName();
            request.Subject.Encode($"CN={this.CN},C={this.C},S={this.S},SERIALNUMBER={this.SerialNumber}");
            request.Encode();

            File.WriteAllText(saveFileDialog.FileName, $"-----BEGIN NEW CERTIFICATE REQUEST-----\r\n{request.RawData}-----END NEW CERTIFICATE REQUEST-----\r\n");

            this.TryClose();
        }
    }
}