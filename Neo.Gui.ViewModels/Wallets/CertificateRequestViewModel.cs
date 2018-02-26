using System;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Wallets;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Wallets;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Wallet;

namespace Neo.Gui.ViewModels.Wallets
{
    public class CertificateRequestViewModel : ViewModelBase, IDialogViewModel<CertificateRequestLoadParameters>
    {
        private readonly ICertificateRequestService certificateRequestService;
        private readonly IFileManager fileManager;
        private readonly IFileDialogService fileDialogService;

        private KeyPair selectedKeyPair;
        private string cn;
        private string c;
        private string s;
        private string serialNumber;

        public CertificateRequestViewModel(
            ICertificateRequestService certificateRequestService,
            IFileManager fileManager,
            IFileDialogService fileDialogService,
            IWalletController walletController)
        {
            this.certificateRequestService = certificateRequestService;
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

        public void OnDialogLoad(CertificateRequestLoadParameters parameters)
        {
        }
        #endregion

        private void RequestCertificate()
        {
            var savedCertificatePath = this.fileDialogService.SaveFileDialog("Certificate Request|*.req|All files|*.*", "req");

            if (string.IsNullOrEmpty(savedCertificatePath)) return;

            var key = this.SelectedKeyPair;

            var certificateBytes = this.certificateRequestService.Request(key, this.CN, this.C, this.S, this.SerialNumber);

            this.fileManager.WriteAllBytes(savedCertificatePath, certificateBytes);

            this.Close(this, EventArgs.Empty);
        }
    }
}