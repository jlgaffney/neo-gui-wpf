using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.UI.Core.Data.TransactionParameters
{
    internal class VotingTransactionConfiguration : ITransactionConfiguration
    {
        public InvocationTransactionType InvocationTransactionType { get; set; }

        public IWalletController WalletController { get; set; }

        public VotingTransactionParameters VotingTransactionParameters { get; set; }
    }
}
