namespace Neo.UI.Core.Wallet.Messages
{
    public class NEP5TokenTotalBalanceSummaryAddedMessage
    {
        public string ScriptHash { get; }

        public string TokenName { get; }

        public string TotalBalance { get; }

        public NEP5TokenTotalBalanceSummaryAddedMessage(string scriptHash, string tokenName, string totalBalance)
        {
            this.ScriptHash = scriptHash;
            this.TokenName = tokenName;
            this.TotalBalance = totalBalance;
        }
    }
}
