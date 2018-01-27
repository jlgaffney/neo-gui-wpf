namespace Neo.UI.Core.Transactions.Parameters
{
    public class ElectionTransactionParameters
    {
        public string BookKeeperPublicKey { get; }

        public ElectionTransactionParameters(string bookKeeperPublicKey)
        {
            this.BookKeeperPublicKey = bookKeeperPublicKey;
        }
    }
}
