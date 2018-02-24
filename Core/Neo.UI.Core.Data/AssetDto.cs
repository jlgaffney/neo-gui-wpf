using Neo.UI.Core.Data.Enums;

namespace Neo.UI.Core.Data
{
    public class AssetDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Decimals { get; set; }

        public TokenType TokenType { get; set; }

        public AssetDto()
        {
            // default value for the Token Type
            this.TokenType = TokenType.FirstClassToken;
        }
    }
}
