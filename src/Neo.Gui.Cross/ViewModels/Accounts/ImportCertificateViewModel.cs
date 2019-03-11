using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Accounts
{
    public class ImportCertificateViewModel :
        ViewModelBase,
        ILoadable
    {
        private readonly IAccountService accountService;
        private readonly ICertificateService certificateService;

        private X509Certificate2 selectedCertificate;

        public ImportCertificateViewModel() { }
        public ImportCertificateViewModel(
            IAccountService accountService,
            ICertificateService certificateService)
        {
            this.accountService = accountService;
            this.certificateService = certificateService;
            
            Certificates = new ObservableCollection<X509Certificate2>();
        }

        public ObservableCollection<X509Certificate2> Certificates { get; }

        public X509Certificate2 SelectedCertificate
        {
            get => selectedCertificate;
            set
            {
                if (Equals(selectedCertificate, value))
                {
                    return;
                }

                selectedCertificate = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(ImportEnabled));
            }
        }

        public bool ImportEnabled => SelectedCertificate != null;

        public ReactiveCommand ImportCommand => ReactiveCommand.Create(Import);
        
        public void Load()
        {
            var storeCertificates = certificateService.GetStoreCertificates();

            Certificates.AddRange(storeCertificates);
        }

        private void Import()
        {
            if (SelectedCertificate == null)
            {
                return;
            }

            var account = accountService.ImportCertificate(SelectedCertificate);

            if (account == null)
            {
                // TODO Inform user

                return;
            }

            OnClose();
        }
    }
}
