using System.Security.Cryptography.X509Certificates;
using Neo.Cryptography.ECC;

namespace Neo.UI.Core.Wallet.Data
{
    internal class AssetInfo
    {
        public UInt256 AssetId { get; }

        public ECPoint AssetOwner { get; }

        public string Name { get; }

        public X509Certificate2 OwnerCertificate { get; set; }

        public bool IssuerCertificateChecked { get; set; }

        public AssetInfo(UInt256 assetId, ECPoint assetOwner, string name)
        {
            this.AssetId = assetId;
            this.AssetOwner = assetOwner;
            this.Name = name;
        }
    }
}
