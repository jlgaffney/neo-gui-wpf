namespace Neo.UI.Core.Wallet.Data
{
    internal class AssetBalance
    {
        public Fixed8 Balance { get; set; }

        public Fixed8 Bonus { get; set; }

        public AssetBalance()
        {
            this.Balance = Fixed8.Zero;
            this.Bonus = Fixed8.Zero;
        }
    }
}
