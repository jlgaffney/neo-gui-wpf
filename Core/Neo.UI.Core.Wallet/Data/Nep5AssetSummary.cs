namespace Neo.UI.Core.Wallet.Data
{
    internal class NEP5AssetSummary : AssetSummary
    {
        public NEP5AssetSummary(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }

        public new string Issuer => $"ScriptHash:{this.ScriptHash}";

        public override string Type => "NEP-5";

        public UInt160 ScriptHash { get; }
    }
}
