using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.UI.Core.Data.TransactionParameters
{
    internal class AssetRegistrationTransactionConfiguration : ITransactionConfiguration
    {
        public InvocationTransactionType InvocationTransactionType { get; set; }

        public IWalletController WalletController { get; set; }

        public AssetRegistrationTransactionParameters AssetRegistrationTransactionParameters { get; set; }
    }
}
