using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Wallets;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Services;

namespace Neo.Gui.ViewModels.Wallets
{
    public class CertificateRequestViewModel : ViewModelBase, IDialogViewModel<CertificateRequestDialogResult>
    {
        private readonly ICertificateService certificateService;
        private readonly IFileManager fileManager;
        private readonly IFileDialogService fileDialogService;

        private KeyPair selectedKeyPair;

        private string cn;
        private string c;
        private string s;

        public CertificateRequestViewModel(
            ICertificateService certificateService,
            IFileManager fileManager,
            IFileDialogService fileDialogService,
            IWalletController walletController)
        {
            this.certificateService = certificateService;
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

        public bool RequestCertificateEnabled => 
            this.SelectedKeyPair != null &&
            !string.IsNullOrEmpty(this.CN) &&
            !string.IsNullOrEmpty(this.C) &&
            !string.IsNullOrEmpty(this.S);

        public RelayCommand RequestCertificateCommand => new RelayCommand(this.RequestCertificate);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<CertificateRequestDialogResult> SetDialogResultAndClose;

        public CertificateRequestDialogResult DialogResult { get; private set; }
        #endregion

        private void RequestCertificate()
        {
            var savedCertificatePath = this.fileDialogService.SaveFileDialog("Certificate Request|*.req|All files|*.*", "req");

            if (string.IsNullOrEmpty(savedCertificatePath)) return;

            var key = this.SelectedKeyPair;

            var certificate = this.certificateService.GenerateCertificate(key, this.CN, this.C, this.S);

            this.fileManager.WriteAllBytes(savedCertificatePath, certificate.Export(X509ContentType.Cert));

            this.Close(this, EventArgs.Empty);
        }
    }
}