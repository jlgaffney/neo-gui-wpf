namespace Neo.UI.Core.Data.TransactionParameters
{
    public class ElectionTransactionParameters
    {
        public string BookKepperPublicKey { get; private set; }

        public ElectionTransactionParameters(string bookKeeperPublicKey)
        {
            this.BookKepperPublicKey = bookKeeperPublicKey;
        }
    }
}
