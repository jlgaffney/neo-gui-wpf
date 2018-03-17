using Neo.Cryptography.ECC;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Globalization.Resources;

namespace Neo.UI.Core.Wallet.Data
{
    internal class AssetInfo
    {
        public UInt256 AssetId { get; }

        public ECPoint AssetOwner { get; }

        public string Issuer { get; private set; }

        public string Name { get; set; }

        public bool IssuerCertificateChecked { get; private set; }

        public AssetInfo(UInt256 assetId, ECPoint assetOwner, string name)
        {
            this.AssetId = assetId;
            this.AssetOwner = assetOwner;
            this.Name = name;

            this.Issuer = $"{Strings.UnknownIssuer}[{assetOwner}]";
        }

        internal void SetIssuerCertificateQueryResult(CertificateQueryResultType resultType, string subject)
        {
            switch (resultType)
            {
                case CertificateQueryResultType.System:
                    //subitem.ForeColor = Color.Green;
                    this.Issuer = Strings.SystemIssuer;
                    break;
                case CertificateQueryResultType.Invalid:
                    //subitem.ForeColor = Color.Red;
                    this.Issuer = $"[{Strings.InvalidCertificate}][{this.AssetOwner}]";
                    break;
                case CertificateQueryResultType.Expired:
                    //subitem.ForeColor = Color.Yellow;
                    this.Issuer = $"[{Strings.ExpiredCertificate}]{subject}[{this.AssetOwner}]";
                    break;
                case CertificateQueryResultType.Good:
                    //subitem.ForeColor = Color.Black;
                    this.Issuer = $"{subject}[{this.AssetOwner}]";
                    break;

                case CertificateQueryResultType.Querying:
                case CertificateQueryResultType.QueryFailed:
                    this.Issuer = $"{Strings.UnknownIssuer}[{this.AssetOwner}]";
                    break;

            }

            switch (resultType)
            {
                case CertificateQueryResultType.System:
                case CertificateQueryResultType.Missing:
                case CertificateQueryResultType.Invalid:
                case CertificateQueryResultType.Expired:
                case CertificateQueryResultType.Good:
                    this.IssuerCertificateChecked = true;
                    break;
            }
        }
    }
}
