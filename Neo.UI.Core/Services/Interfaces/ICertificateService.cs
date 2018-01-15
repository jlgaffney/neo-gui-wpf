using Neo.Cryptography.ECC;
using Neo.UI.Core.Certificates;

namespace Neo.UI.Core.Services.Interfaces
{
    internal interface ICertificateService
    {
        void Initialize(string certificateCachePath);

        CertificateQueryResult Query(ECPoint publickey);

        string GetCachedCertificatePath(ECPoint publicKey);
    }
}
