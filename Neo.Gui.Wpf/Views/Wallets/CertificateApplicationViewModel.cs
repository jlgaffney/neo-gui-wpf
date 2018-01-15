using System;
using System.Linq;
using System.Text;

using CERTENROLLLib;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Wallets;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Wallets;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public class CertificateApplicationViewModel : ViewModelBase, IDialogViewModel<CertificateApplicationLoadParameters>
    {
        private readonly IFileManager fileManager;
        private readonly IFileDialogService fileDialogService;

        private KeyPair selectedKeyPair;
        private string cn;
        private string c;
        private string s;
        private string serialNumber;

        public CertificateApplicationViewModel(
            IFileManager fileManager,
            IFileDialogService fileDialogService,
            IWalletController walletController)
        {
            this.fileManager = fileManager;
            this.fileDialogService = fileDialogService;

            this.KeyPairs = walletController.GetStandardAccounts()
                .Select(p => p.GetKey()).ToArray();
        }

        public KeyPair[] KeyPairs { get; }

        public KeyPair SelectedKeyPair
        {
            get => this.selectedKeyPair;
            set
            {
                if (Equals(this.selectedKeyPair, value)) return;

                this.selectedKeyPair = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public string CN
        {
            get => this.cn;
            set
            {
                if (this.cn == value) return;

                this.cn = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public string C
        {
            get => this.c;
            set
            {
                if (this.c == value) return;

                this.c = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public string S
        {
            get => this.s;
            set
            {
                if (this.s == value) return;

                this.s = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public string SerialNumber
        {
            get => this.serialNumber;
            set
            {
                if (this.serialNumber == value) return;

                this.serialNumber = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.RequestCertificateEnabled));
            }
        }

        public bool RequestCertificateEnabled => 
            this.SelectedKeyPair != null &&
            !string.IsNullOrEmpty(this.CN) &&
            !string.IsNullOrEmpty(this.C) &&
            !string.IsNullOrEmpty(this.S) &&
            !string.IsNullOrEmpty(this.SerialNumber);

        public RelayCommand RequestCertificateCommand => new RelayCommand(this.RequestCertificate);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(CertificateApplicationLoadParameters parameters)
        {
        }
        #endregion

        private void RequestCertificate()
        {
            var savedCertificatePath = this.fileDialogService.SaveFileDialog("Certificate Request|*.req|All files|*.*", "req");

            if (string.IsNullOrEmpty(savedCertificatePath)) return;

            var key = this.SelectedKeyPair;
            var publicKey = key.PublicKey.EncodePoint(false).Skip(1).ToArray();

            byte[] privateKey;
            using (key.Decrypt())
            {
                const int ECDSA_PRIVATE_P256_MAGIC = 0x32534345;
                privateKey = BitConverter.GetBytes(ECDSA_PRIVATE_P256_MAGIC).Concat(BitConverter.GetBytes(32)).Concat(publicKey).Concat(key.PrivateKey).ToArray();
            }

            var x509Key = new CX509PrivateKey();

            // Set property using Reflection so this project can compile if this property isn't available
            var property = typeof(CX509PrivateKey).GetProperty("AlgorithmName");

            if (property == null)
            {
                // TODO Find a way to generate a certificate without setting this property
            }
            else
            {
                property.SetValue(x509Key, "ECDSA_P256", null);
            }

            x509Key.Import("ECCPRIVATEBLOB", Convert.ToBase64String(privateKey));

            Array.Clear(privateKey, 0, privateKey.Length);

            var request = new CX509CertificateRequestPkcs10();

            request.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, x509Key, null);
            request.Subject = new CX500DistinguishedName();
            request.Subject.Encode($"CN={this.CN},C={this.C},S={this.S},SERIALNUMBER={this.SerialNumber}");
            request.Encode();

            var certificateText = "-----BEGIN NEW CERTIFICATE REQUEST-----\r\n" + request.RawData + "-----END NEW CERTIFICATE REQUEST-----\r\n";
            var certificateBytes = Encoding.UTF8.GetBytes(certificateText);

            this.fileManager.WriteAllBytes(savedCertificatePath, certificateBytes);

            this.Close(this, EventArgs.Empty);
        }
    }
}