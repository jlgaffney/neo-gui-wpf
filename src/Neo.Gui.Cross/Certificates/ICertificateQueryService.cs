using Neo.Cryptography.ECC;

namespace Neo.Gui.Cross.Certificates
{
    public interface ICertificateQueryService
    {
        CertificateQueryResult Query(ECPoint pubkey);

        CertificateQueryResult Query(UInt160 hash);
    }
}
