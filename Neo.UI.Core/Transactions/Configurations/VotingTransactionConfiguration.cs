using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Transactions;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Data.TransactionParameters
{
    internal class VotingTransactionConfiguration : ITransactionConfiguration
    {
        public InvocationTransactionType InvocationTransactionType { get; set; }

        public IWalletController WalletController { get; set; }

        public VotingTransactionParameters VotingTransactionParameters { get; set; }
    }
}
