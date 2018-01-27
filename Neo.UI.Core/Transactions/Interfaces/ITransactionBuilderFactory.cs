using System.Collections.Generic;

using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Interfaces
{
    public interface ITransactionBuilderFactory
    {
        ITransactionBuilder GetTransactionInvoker(
            IWalletController walletController,
            IEnumerable<ITransactionBuilder> transactionInvokers,
            InvocationTransactionType invocationTransactionType,
            AssetRegistrationTransactionParameters assetRegistrationParameters,
            AssetTransferTransactionParameters assetTransferTransactionParameters,
            DeployContractTransactionParameters deployContractTransactionParameters,
            ElectionTransactionParameters electionTransactionParameters,
            VotingTransactionParameters votingTransactionParameters);
    }
}
