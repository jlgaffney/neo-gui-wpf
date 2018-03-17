namespace Neo.UI.Core.Wallet.Data
{
    internal class NEP5TokenInfo
    {
        public NEP5TokenInfo(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }

        public string Name { get; set; }

        public string Issuer => $"ScriptHash:{this.ScriptHash}";

        public UInt160 ScriptHash { get; }
    }
}
