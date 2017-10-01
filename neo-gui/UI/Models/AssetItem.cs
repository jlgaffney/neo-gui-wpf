using Neo.Core;

namespace Neo.UI.Models
{
    public class AssetItem
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Issuer { get; set; }

        public string Value { get; set; }

        public AssetState State { get; set; }
    }
}