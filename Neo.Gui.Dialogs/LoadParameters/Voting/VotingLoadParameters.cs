namespace Neo.Gui.Dialogs.LoadParameters.Voting
{
    public class VotingLoadParameters
    {
        public string VoterScriptHash { get; }

        public VotingLoadParameters(string voterScriptHash)
        {
            this.VoterScriptHash = voterScriptHash;
        }
    }
}
