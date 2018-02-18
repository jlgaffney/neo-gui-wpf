namespace Neo.UI.Core.Transactions.Parameters
{
    public class VotingTransactionParameters : TransactionParameters
    {
        public string ScriptHash { get; }

        public string[] Votes { get; }

        public VotingTransactionParameters(string scriptHash, string[] votes)
        {
            this.ScriptHash = scriptHash;
            this.Votes = votes;
        }
    }
}
