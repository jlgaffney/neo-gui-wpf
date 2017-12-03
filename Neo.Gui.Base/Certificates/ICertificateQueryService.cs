using ECPoint = Neo.Cryptography.ECC.ECPoint;

namespace Neo.Gui.Base.Certificates
{
    public interface ICertificateQueryService
    {
        void Initialize(string certificateCachePath);

        CertificateQueryResult Query(ECPoint pubkey);

        CertificateQueryResult Query(UInt160 hash);
    }
}
