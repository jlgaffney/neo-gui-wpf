using System.Collections.Generic;
using System.Linq;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Transactions.Parameters
{
    public class AssetDistributionTransactionParameters : TransactionParameters
    {
        public string AssetId { get; }

        public IReadOnlyList<TransactionOutputItem> TransactionOutputItems { get; }

        public AssetDistributionTransactionParameters(string assetId, IEnumerable<TransactionOutputItem> transactionOutputItems)
        {
            this.AssetId = assetId;
            this.TransactionOutputItems = transactionOutputItems.ToList();
        }
    }
}
