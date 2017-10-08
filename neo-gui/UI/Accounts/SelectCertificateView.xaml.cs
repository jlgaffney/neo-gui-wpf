using System.Security.Cryptography.X509Certificates;

namespace Neo.UI.Accounts
{
    public partial class SelectCertificateView
    {
        private readonly SelectCertificateViewModel viewModel;

        public SelectCertificateView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as SelectCertificateViewModel;
        }

        public X509Certificate2 SelectedCertificate => this.viewModel?.SelectedCertificate;
    }
}