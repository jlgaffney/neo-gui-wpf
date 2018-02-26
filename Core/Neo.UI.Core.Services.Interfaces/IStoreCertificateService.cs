using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface IStoreCertificateService
    {
        IEnumerable<X509Certificate2> GetStoreCertificates();
    }
}
