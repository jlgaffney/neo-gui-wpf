namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class VotingParameters
    {
        public string ScriptHash { get; private set; }

        public string Votes { get; private set; }

        public VotingParameters(string scriptHash, string votes)
        {
            this.ScriptHash = scriptHash;
            this.Votes = votes;
        }
    }
}
