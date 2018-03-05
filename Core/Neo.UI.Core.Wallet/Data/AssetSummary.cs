namespace Neo.UI.Core.Wallet.Data
{
    internal abstract class AssetSummary
    {
        public string Name { get; set; }

        public string Issuer { get; set; }

        public string TotalBalance { get; set; }

        public abstract string Type { get; }
    }
}
