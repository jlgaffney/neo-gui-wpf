using Neo.Cryptography.ECC;

using Neo.Gui.Base.Certificates;

namespace Neo.Gui.Base.Services.Interfaces
{
    internal interface ICertificateService
    {
        void Initialize(string certificateCachePath);

        CertificateQueryResult Query(ECPoint publickey);

        string GetCachedCertificatePath(ECPoint publicKey);
    }
}
