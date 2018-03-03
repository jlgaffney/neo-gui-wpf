namespace Neo.UI.Core.Wallet.Data
{
    internal class AccountBalanceSummary
    {
        public AccountBalanceSummary()
        {
            this.Neo = Fixed8.Zero;
            this.Gas = Fixed8.Zero;
        }

        public Fixed8 Neo { get; set; }

        public Fixed8 Gas { get; set; }
    }
}
