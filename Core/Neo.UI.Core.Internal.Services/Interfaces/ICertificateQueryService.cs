using Neo.Cryptography.ECC;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Internal.Services.Interfaces
{
    public interface ICertificateQueryService
    {
        void Initialize(string certificateCachePath);

        CertificateQueryResult Query(ECPoint publickey);

        string GetCachedCertificatePath(ECPoint publicKey);
    }
}
