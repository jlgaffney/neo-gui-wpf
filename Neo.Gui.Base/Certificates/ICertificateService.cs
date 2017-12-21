using Neo.Cryptography.ECC;

namespace Neo.Gui.Base.Certificates
{
    public interface ICertificateService
    {
        void Initialize(string certificateCachePath);

        CertificateQueryResult Query(ECPoint publickey);

        string GetCachedCertificatePath(ECPoint publicKey);
    }
}
