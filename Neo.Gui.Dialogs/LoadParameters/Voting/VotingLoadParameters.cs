namespace Neo.Gui.Dialogs.LoadParameters.Voting
{
    public class VotingLoadParameters
    {
        public string ScriptHash { get; }

        public VotingLoadParameters(string scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
