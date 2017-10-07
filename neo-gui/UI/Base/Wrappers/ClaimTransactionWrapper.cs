using System.Collections.Generic;
using System.Linq;
using Neo.Core;

namespace Neo.UI.Base.Wrappers
{
    internal class ClaimTransactionWrapper : TransactionWrapper
    {
        public List<CoinReferenceWrapper> Claims { get; set; } = new List<CoinReferenceWrapper>();

        public override Transaction Unwrap()
        {
            var tx = (ClaimTransaction) base.Unwrap();
            tx.Claims = Claims.Select(p => p.Unwrap()).ToArray();
            return tx;
        }
    }
}
