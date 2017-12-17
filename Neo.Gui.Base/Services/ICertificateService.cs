using System.Security.Cryptography.X509Certificates;

using Neo.Cryptography.ECC;
using Neo.Wallets;

using Neo.Gui.Base.Certificates;

namespace Neo.Gui.Base.Services
{
    public interface ICertificateService
    {
        void Initialize(string certificateCachePath);

        CertificateQueryResult GetCertificate(ECPoint publickey);

        bool ViewCertificate(ECPoint publicKey);

        X509Certificate2 GenerateCertificate(KeyPair key, string cn, string c, string s);
    }
}
