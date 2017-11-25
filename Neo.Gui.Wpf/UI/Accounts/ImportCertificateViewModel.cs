using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;

namespace Neo.UI.Accounts
{
    public class ImportCertificateViewModel : ViewModelBase
    {
        private readonly IMessagePublisher messagePublisher;

        private X509Certificate2 selectedCertificate;

        public ImportCertificateViewModel(
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;

            // Load certificates
            using (var store = new X509Store())
            {
                store.Open(OpenFlags.ReadOnly);

                this.Certificates = new ObservableCollection<X509Certificate2>(
                    store.Certificates.Cast<X509Certificate2>());
            }
        }

        public ObservableCollection<X509Certificate2> Certificates { get; }

        public X509Certificate2 SelectedCertificate
        {
            get => this.selectedCertificate;
            set
            {
                if (Equals(this.selectedCertificate, value)) return;

                this.selectedCertificate = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }
        
        public bool OkEnabled => this.SelectedCertificate != null;

        public ICommand OkCommand => new RelayCommand(this.Ok);


        private void Ok()
        {
            if (this.SelectedCertificate == null) return;

            this.messagePublisher.Publish(new ImportCertificateMessage(this.SelectedCertificate));
            this.TryClose();
        }
    }
}