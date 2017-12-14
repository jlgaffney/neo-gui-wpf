namespace Neo.Gui.Base.Dialogs.LoadParameters.Voting
{
    public class VotingLoadParameters
    {
        public UInt160 ScriptHash { get; }

        public VotingLoadParameters(UInt160 scriptHash)
        {
            this.ScriptHash = scriptHash;
        }
    }
}
