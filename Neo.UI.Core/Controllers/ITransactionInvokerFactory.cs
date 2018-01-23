using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.UI.Core.Controllers
{
    public interface ITransactionInvokerFactory
    {
        ITransactionInvoker GetTransactionInvoker(
            IWalletController walletController,
            InvocationTransactionType invocationTransactionType,
            AssetRegistrationTransactionParameters assetRegistrationParameters,
            AssetTransferTransactionParameters assetTransferTransactionParameters,
            DeployContractTransactionParameters deployContractTransactionParameters,
            ElectionTransactionParameters electionTransactionParameters,
            VotingTransactionParameters votingTransactionParameters);
    }
}
