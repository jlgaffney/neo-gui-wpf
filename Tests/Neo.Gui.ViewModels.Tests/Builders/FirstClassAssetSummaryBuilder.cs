using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.UI.Core.Data;

namespace Neo.Gui.ViewModels.Tests.Builders
{
    // TODO Add NEP5AssetItemBuilder class
    public class FirstClassAssetSummaryBuilder
    {
        private string internalName = "Name";
        private string internalValue = "Value";
        private string internalIssuer = "Issuer";
        private string internalAssetId = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";
        private ECPoint internalAssetOwner = new ECPoint();
        private AssetType internalAssetType = AssetType.Token;

        public FirstClassAssetSummaryBuilder WithName(string name)
        {
            this.internalName = name;
            return this;
        }

        public FirstClassAssetSummaryBuilder WithValue(string value)
        {
            this.internalValue = value;
            return this;
        }

        public FirstClassAssetSummaryBuilder WithIssuer(string issuer)
        {
            this.internalIssuer = issuer;
            return this;
        }

        public FirstClassAssetSummaryBuilder WithGoverningToken()
        {
            this.internalAssetType = AssetType.GoverningToken;
            return this;
        }

        public FirstClassAssetSummaryBuilder WithUtilityToken()
        {
            this.internalAssetType = AssetType.UtilityToken;
            return this;
        }

        public FirstClassAssetSummaryBuilder WithCustomToken()
        {
            this.internalAssetOwner = new ECPoint();
            return this;
        }

        public FirstClassAssetSummaryBuilder WithAssetId(string assetId)
        {
            this.internalAssetId = assetId;
            return this;
        }

        public AssetSummary Build()
        {
            return new FirstClassAssetSummary(this.internalAssetId, this.internalAssetOwner, this.internalAssetType)
            {
                Name = this.internalName,
                Issuer = this.internalIssuer,
                TotalBalance = this.internalValue
            };
        }
    }
}
