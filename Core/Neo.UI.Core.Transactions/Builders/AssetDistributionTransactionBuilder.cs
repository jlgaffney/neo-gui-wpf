using System.Linq;
using Neo.Core;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Builders
{
    public class AssetDistributionTransactionBuilder : ITransactionBuilder<AssetDistributionTransactionParameters>
    {
        public Transaction Build(AssetDistributionTransactionParameters parameters)
        {
            var assetId = parameters.AssetId;
            var items = parameters.TransactionOutputItems;

            return new IssueTransaction
            {
                Version = 1,
                Outputs = items.GroupBy(p => p.ScriptHash).Select(g => new TransactionOutput
                {
                    AssetId = UInt256.Parse(assetId),
                    Value = g.Sum(p => new Fixed8((long)p.Value.Value)),
                    ScriptHash = g.Key
                }).ToArray()
            };
        }
    }
}
