using System.Collections.ObjectModel;
using System.Linq;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Services;
using Neo.Wallets;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Wallets
{
    public class CertificateRequestViewModel :
        ViewModelBase,
        ILoadable
    {
        private readonly IAccountService accountService;
        private readonly ICertificateService certificateService;
        private readonly IFileDialogService fileDialogService;
        private readonly IFileService fileService;

        private KeyPair selectedKeyPair;
        private string cn;
        private string c;
        private string s;
        private string serialNumber;

        public CertificateRequestViewModel() { }
        public CertificateRequestViewModel(
            IAccountService accountService,
            ICertificateService certificateService,
            IFileDialogService fileDialogService,
            IFileService fileService)
        {
            this.accountService = accountService;
            this.certificateService = certificateService;
            this.fileDialogService = fileDialogService;
            this.fileService = fileService;

            KeyPairs = new ObservableCollection<KeyPair>();
        }

        public ObservableCollection<KeyPair> KeyPairs { get; }

        public KeyPair SelectedKeyPair
        {
            get => selectedKeyPair;
            set
            {
                if (Equals(selectedKeyPair, value))
                {
                    return;
                }

                selectedKeyPair = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(RequestCertificateEnabled));
            }
        }

        public string CN
        {
            get => cn;
            set
            {
                if (Equals(cn, value))
                {
                    return;
                }

                cn = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(RequestCertificateEnabled));
            }
        }

        public string C
        {
            get => c;
            set
            {
                if (Equals(c, value))
                {
                    return;
                }

                c = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(RequestCertificateEnabled));
            }
        }

        public string S
        {
            get => s;
            set
            {
                if (Equals(s, value))
                {
                    return;
                }

                s = value;

                this.RaisePropertyChanged();
                
                this.RaisePropertyChanged(nameof(RequestCertificateEnabled));
            }
        }

        public string SerialNumber
        {
            get => serialNumber;
            set
            {
                if (Equals(serialNumber, value))
                {
                    return;
                }

                serialNumber = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(RequestCertificateEnabled));
            }
        }

        public bool RequestCertificateEnabled =>
            SelectedKeyPair != null &&
            !string.IsNullOrEmpty(CN) &&
            !string.IsNullOrEmpty(C) &&
            !string.IsNullOrEmpty(S) &&
            !string.IsNullOrEmpty(SerialNumber);

        public ReactiveCommand RequestCertificateCommand => ReactiveCommand.Create(RequestCertificate);

        public ReactiveCommand CancelCommand => ReactiveCommand.Create(OnClose);
        
        public void Load()
        {
            KeyPairs.AddRange(accountService.GetStandardAccounts().Select(p => p.GetKey()).ToArray());
        }

        private async void RequestCertificate()
        {
            var savedCertificatePath = await fileDialogService.SaveFileDialog();
            //"Certificate Request|*.req|All files|*.*", "req");

            if (string.IsNullOrEmpty(savedCertificatePath))
            {
                return;
            }

            var certificateBytes = certificateService.CreateCertificate(SelectedKeyPair, CN, C, S, SerialNumber);

            fileService.WriteAllBytes(savedCertificatePath, certificateBytes);

            OnClose();
        }
    }
}
