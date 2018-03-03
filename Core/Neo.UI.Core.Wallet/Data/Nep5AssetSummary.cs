namespace Neo.UI.Core.Data
{
    public class NEP5AssetSummary : AssetSummary
    {
        public NEP5AssetSummary(string scriptHash)
        {
            this.ScriptHash = scriptHash;
        }

        public new string Issuer => $"ScriptHash:{this.ScriptHash}";

        public override string Type => "NEP-5";

        public string ScriptHash { get; }
    }
}
