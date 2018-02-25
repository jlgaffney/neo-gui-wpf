using System.Collections.Generic;
using Neo.Core;

namespace Neo.UI.Core.Transactions.Parameters
{
    public class ClaimTransactionParameters : TransactionParameters
    {
        public IReadOnlyList<CoinReference> Claims { get; }

        public UInt256 AssetId { get; }

        public Fixed8 Amount { get; }

        public UInt160 ChangeAddress { get; }

        public ClaimTransactionParameters(CoinReference[] claims, UInt256 assetId, Fixed8 amount, UInt160 changeAddress)
        {
            this.Claims = claims;
            this.AssetId = assetId;
            this.Amount = amount;
            this.ChangeAddress = changeAddress;
        }
    }
}
