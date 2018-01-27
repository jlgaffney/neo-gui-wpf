using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Transactions;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Data.TransactionParameters
{
    internal class AssetRegistrationTransactionConfiguration : ITransactionConfiguration
    {
        public InvocationTransactionType InvocationTransactionType { get; set; }

        public IWalletController WalletController { get; set; }

        public AssetRegistrationTransactionParameters AssetRegistrationTransactionParameters { get; set; }
    }
}
