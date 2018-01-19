namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class ElectionParameters
    {
        public string BookKepperPublicKey { get; private set; }

        public ElectionParameters(string bookKeeperPublicKey)
        {
            this.BookKepperPublicKey = bookKeeperPublicKey;
        }
    }
}
