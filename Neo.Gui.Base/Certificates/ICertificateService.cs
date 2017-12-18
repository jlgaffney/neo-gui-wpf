using Neo.Cryptography.ECC;

namespace Neo.Gui.Base.Certificates
{
    public interface ICertificateService
    {
        void Initialize(string certificateCachePath);

        CertificateQueryResult Query(ECPoint publickey);

        bool ViewCertificate(ECPoint publicKey);
    }
}
