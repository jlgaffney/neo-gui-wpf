using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Neo.Gui.Base.Services.Interfaces
{
    public interface IStoreCertificateService
    {
        IEnumerable<X509Certificate2> GetStoreCertificates();
    }
}
