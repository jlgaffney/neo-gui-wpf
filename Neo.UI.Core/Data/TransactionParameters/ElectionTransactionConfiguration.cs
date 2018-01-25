using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.UI.Core.Data.TransactionParameters
{
    public class ElectionTransactionConfiguration : ITransactionConfiguration
    {
        public InvocationTransactionType InvocationTransactionType { get; set; }

        public IWalletController WalletController { get; set; }

        public ElectionTransactionParameters ElectionTransactionParameters { get; set; }
    }
}
