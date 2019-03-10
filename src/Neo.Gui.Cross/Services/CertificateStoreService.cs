using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Neo.Gui.Cross.Services
{
    public class CertificateStoreService : ICertificateStoreService
    {
        public IEnumerable<X509Certificate2> GetCertificates()
        {
            using (var store = new X509Store())
            {
                store.Open(OpenFlags.ReadOnly);

                return store.Certificates
                    .Cast<X509Certificate2>()
                    .ToList();
            }
        }
    }
}
