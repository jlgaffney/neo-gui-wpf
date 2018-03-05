using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Globalization.Resources;

namespace Neo.UI.Core.Wallet.Data
{
    internal class FirstClassAssetSummary : AssetSummary
    {
        public bool IssuerCertificateChecked { get; private set; }

        public bool IsSystemAsset =>
            this.AssetType == AssetType.GoverningToken ||
            this.AssetType == AssetType.UtilityToken;

        public override string Type => this.AssetType.ToString();

        public UInt256 AssetId { get; }

        public ECPoint AssetOwner { get; }

        public AssetType AssetType { get; }

        public FirstClassAssetSummary(UInt256 assetId, ECPoint assetOwner, AssetType assetType)
        {
            this.AssetId = assetId;
            this.AssetOwner = assetOwner;
            this.AssetType = assetType;
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
