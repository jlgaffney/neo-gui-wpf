using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Transactions;

namespace Neo.UI.Core.Data.TransactionParameters
{
    public interface ITransactionConfiguration
    {
        InvocationTransactionType InvocationTransactionType { get; }

        IWalletController WalletController { get; set; }
    }
}
