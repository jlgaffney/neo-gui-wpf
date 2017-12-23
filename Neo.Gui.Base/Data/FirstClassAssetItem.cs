using Neo.Core;
using Neo.Gui.Base.Certificates;
using Neo.Gui.Base.Globalization;

namespace Neo.Gui.Base.Data
{
    public class FirstClassAssetItem : AssetItem
    {
        public bool IssuerCertificateChecked { get; private set; }

        public bool IsSystemAsset => this.State != null &&
            (this.State.AssetType == AssetType.GoverningToken ||
                this.State.AssetType == AssetType.UtilityToken);

        public AssetState State { get; set; }

        public void SetIssuerCertificateQueryResult(CertificateQueryResult queryResult)
        {
            if (queryResult == null) return;

            using (queryResult)
            {
                switch (queryResult.Type)
                {
                    case CertificateQueryResultType.Querying:
                    case CertificateQueryResultType.QueryFailed:
                        break;
                    case CertificateQueryResultType.System:
                        //subitem.ForeColor = Color.Green;
                        this.Issuer = Strings.SystemIssuer;
                        break;
                    case CertificateQueryResultType.Invalid:
                        //subitem.ForeColor = Color.Red;
                        this.Issuer = $"[{Strings.InvalidCertificate}][{this.State.Owner}]";
                        break;
                    case CertificateQueryResultType.Expired:
                        //subitem.ForeColor = Color.Yellow;
                        this.Issuer = $"[{Strings.ExpiredCertificate}]{queryResult.Certificate.Subject}[{this.State.Owner}]";
                        break;
                    case CertificateQueryResultType.Good:
                        //subitem.ForeColor = Color.Black;
                        this.Issuer = $"{queryResult.Certificate.Subject}[{this.State.Owner}]";
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
