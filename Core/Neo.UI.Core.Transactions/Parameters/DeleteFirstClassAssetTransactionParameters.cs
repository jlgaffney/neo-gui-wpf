namespace Neo.UI.Core.Transactions.Parameters
{
    public class DeleteFirstClassAssetTransactionParameters : TransactionParameters
    {
        public UInt256 AssetId { get; }

        public Fixed8 Amount { get; }

        public DeleteFirstClassAssetTransactionParameters(UInt256 assetId, Fixed8 amount)
        {
            this.AssetId = assetId;
            this.Amount = amount;
        }
    }
}
