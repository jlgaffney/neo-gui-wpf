using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Neo.Gui.Cross.Services
{
    public interface ICertificateStoreService
    {
        IEnumerable<X509Certificate2> GetCertificates();
    }
}
