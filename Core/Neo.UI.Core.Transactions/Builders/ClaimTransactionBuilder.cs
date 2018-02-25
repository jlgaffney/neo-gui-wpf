using System.Linq;
using Neo.Core;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Builders
{
    public class ClaimTransactionBuilder : ITransactionBuilder<ClaimTransactionParameters>
    {
        public Transaction Build(ClaimTransactionParameters parameters)
        {
            var claims = parameters.Claims;
            var claimAssetId = parameters.AssetId;
            var claimAmount = parameters.Amount;
            var changeAddress = parameters.ChangeAddress;

            return new ClaimTransaction
            {
                Claims = claims.ToArray(),
                Attributes = new TransactionAttribute[0],
                Inputs = new CoinReference[0],
                Outputs = new[]
                {
                    new TransactionOutput
                    {
                        AssetId = claimAssetId,
                        Value = claimAmount,
                        ScriptHash = changeAddress
                    }
                }
            };
        }
    }
}
