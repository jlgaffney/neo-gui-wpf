namespace Neo.UI.Core.Data.TransactionParameters
{
    public class ElectionTransactionParameters
    {
        public string BookKeeperPublicKey { get; private set; }

        public ElectionTransactionParameters(string bookKeeperPublicKey)
        {
            this.BookKeeperPublicKey = bookKeeperPublicKey;
        }
    }
}
