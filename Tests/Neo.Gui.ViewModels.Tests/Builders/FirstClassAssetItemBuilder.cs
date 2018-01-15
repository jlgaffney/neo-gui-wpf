using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.UI.Core.Data;

namespace Neo.Gui.ViewModels.Tests.Builders
{
    // TODO Add NEP5AssetItemBuilder class
    public class FirstClassAssetItemBuilder
    {
        private string internalName = "Name";
        private string internalValue = "Value";
        private string internalIssuer = "Issuer";
        private UInt256 internalAssetId = UInt256.Parse("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");
        private ECPoint internalAssetOwner = new ECPoint();
        private AssetType internalAssetType = AssetType.Token;

        public FirstClassAssetItemBuilder WithName(string name)
        {
            this.internalName = name;
            return this;
        }

        public FirstClassAssetItemBuilder WithValue(string value)
        {
            this.internalValue = value;
            return this;
        }

        public FirstClassAssetItemBuilder WithIssuer(string issuer)
        {
            this.internalIssuer = issuer;
            return this;
        }

        public FirstClassAssetItemBuilder WithGoverningToken()
        {
            this.internalAssetType = AssetType.GoverningToken;
            return this;
        }

        public FirstClassAssetItemBuilder WithUtilityToken()
        {
            this.internalAssetType = AssetType.UtilityToken;
            return this;
        }

        public FirstClassAssetItemBuilder WithCustomToken()
        {
            this.internalAssetOwner = new ECPoint();
            return this;
        }

        public FirstClassAssetItemBuilder WithAssetId(UInt256 assetId)
        {
            this.internalAssetId = assetId;
            return this;
        }

        public AssetItem Build()
        {
            return new FirstClassAssetItem(this.internalAssetId, this.internalAssetOwner, this.internalAssetType, this.internalValue)
            {
                Name = this.internalName,
                Issuer = this.internalIssuer
            };
        }
    }
}
