using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Gui.Globalization.Resources;
using Neo.UI.Core.Certificates;

namespace Neo.UI.Core.Data
{
    public class FirstClassAssetItem : AssetItem
    {
        public FirstClassAssetItem(UInt256 assetId, ECPoint assetOwner, AssetType assetType, string value)
        {
            this.AssetId = assetId;
            this.AssetOwner = assetOwner;
            this.AssetType = assetType;

            this.Value = value;
        }

        public bool IssuerCertificateChecked { get; private set; }

        public bool IsSystemAsset => 
            this.AssetType == AssetType.GoverningToken ||
            this.AssetType == AssetType.UtilityToken;

        public override string Type => this.AssetType.ToString();

        public override string Value { get; }

        public UInt256 AssetId { get; }

        public ECPoint AssetOwner { get; }

        public AssetType AssetType { get; }

        internal void SetIssuerCertificateQueryResult(CertificateQueryResult queryResult)
        {
            if (queryResult == null) return;

            using (queryResult)
            {
                switch (queryResult.Type)
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
                        this.Issuer = $"[{Strings.ExpiredCertificate}]{queryResult.Certificate.Subject}[{this.AssetOwner}]";
                        break;
                    case CertificateQueryResultType.Good:
                        //subitem.ForeColor = Color.Black;
                        this.Issuer = $"{queryResult.Certificate.Subject}[{this.AssetOwner}]";
                        break;

                    case CertificateQueryResultType.Querying:
                    case CertificateQueryResultType.QueryFailed:
                        this.Issuer = $"{Strings.UnknownIssuer}[{this.AssetOwner}]";
                        break;

                }

                switch (queryResult.Type)
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
}
