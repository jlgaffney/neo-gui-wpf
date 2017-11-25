using System.Security.Cryptography.X509Certificates;

namespace Neo.Gui.Wpf.Messages
{
    public class ImportCertificateMessage
    {
        public ImportCertificateMessage(X509Certificate2 selectedCertificate)
        {
            this.SelectedCertificate = selectedCertificate;
        }

        public X509Certificate2 SelectedCertificate { get; }
    }
}