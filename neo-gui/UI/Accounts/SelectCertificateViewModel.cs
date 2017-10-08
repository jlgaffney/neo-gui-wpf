using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Accounts
{
    public class SelectCertificateViewModel : ViewModelBase
    {
        private X509Certificate2 selectedCertificate;

        public SelectCertificateViewModel()
        {
            // Load certificates
            using (var store = new X509Store())
            {
                store.Open(OpenFlags.ReadOnly);

                this.Certificates = new ObservableCollection<X509Certificate2>();

                foreach (var certificate in store.Certificates)
                {
                    this.Certificates.Add(certificate);
                }
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

        public ICommand OkCommand => new RelayCommand(this.TryClose);
    }
}