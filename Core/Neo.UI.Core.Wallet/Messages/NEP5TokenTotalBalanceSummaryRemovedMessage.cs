namespace Neo.UI.Core.Wallet.Messages
{
    public class NEP5TokenTotalBalanceSummaryRemovedMessage
    {
        public string ScriptHash { get; }
        
        public NEP5TokenTotalBalanceSummaryRemovedMessage(string scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
