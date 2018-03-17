namespace Neo.UI.Core.Wallet.Messages
{
    public class NEP5TokenTotalBalanceChangedMessage
    {
        public string ScriptHash { get; }

        public string TotalBalance { get; }

        public NEP5TokenTotalBalanceChangedMessage(string scriptHash, string totalBalance)
        {
            this.ScriptHash = scriptHash;
            this.TotalBalance = totalBalance;
        }
    }
}
