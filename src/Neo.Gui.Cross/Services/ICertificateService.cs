using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Neo.Wallets;

namespace Neo.Gui.Cross.Services
{
    public interface ICertificateService
    {
        IEnumerable<X509Certificate2> GetStoreCertificates();

        byte[] CreateCertificate(KeyPair keyPair, string cn, string c, string s, string serialNumber);
    }
}
