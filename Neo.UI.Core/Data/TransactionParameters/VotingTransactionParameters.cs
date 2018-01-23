namespace Neo.UI.Core.Data.TransactionParameters
{
    public class VotingTransactionParameters
    {
        public string ScriptHash { get; private set; }

        public string Votes { get; private set; }

        public VotingTransactionParameters(string scriptHash, string votes)
        {
            this.ScriptHash = scriptHash;
            this.Votes = votes;
        }
    }
}
