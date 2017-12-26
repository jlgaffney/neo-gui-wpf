namespace Neo.Gui.Base.Data
{
    public class NEP5AssetItem : AssetItem
    {
        public NEP5AssetItem(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }

        public new string Issuer => $"ScriptHash:{this.ScriptHash}";

        public override string Type => "NEP-5";

        public UInt160 ScriptHash { get; }
    }
}
