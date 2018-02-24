namespace Neo.UI.Core.Transactions.Parameters
{
    public class ElectionTransactionParameters : TransactionParameters
    {
        public string ValidatorPublicKey { get; }

        public ElectionTransactionParameters(string validatorPublicKey)
        {
            this.ValidatorPublicKey = validatorPublicKey;
        }
    }
}
