using System.Numerics;

namespace Neo.UI.Core.Data
{
    public class NEP5AssetItem : AssetItem
    {
        private BigDecimal balance;

        public NEP5AssetItem(string scriptHash, BigDecimal balance)
        {
            this.ScriptHash = scriptHash;
            this.balance = balance;
        }

        public new string Issuer => $"ScriptHash:{this.ScriptHash}";

        public override string Type => "NEP-5";

        public override string Value => this.balance.ToString();

        public string ScriptHash { get; }

        public bool BalanceIsZero => this.balance.Value == BigInteger.Zero;
    }
}
