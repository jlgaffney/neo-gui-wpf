using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.UI.Core.Data.TransactionParameters
{
    public class AssetTransferTransactionConfiguration : ITransactionConfiguration
    {
        public InvocationTransactionType InvocationTransactionType { get; set; }

        public IWalletController WalletController { get; set; }

        public AssetTransferTransactionParameters AssetTransferTransactionParameters { get; set; }
    }
}
