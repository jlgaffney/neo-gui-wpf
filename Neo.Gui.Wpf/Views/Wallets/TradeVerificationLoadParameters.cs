using System.Collections.Generic;
using Neo.Core;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public class TradeVerificationLoadParameters
    {
        public IEnumerable<TransactionOutput> TransactionOutputs { get; private set; }

        public TradeVerificationLoadParameters(IEnumerable<TransactionOutput> outputs)
        {
            this.TransactionOutputs = outputs;
        }
    }
}
