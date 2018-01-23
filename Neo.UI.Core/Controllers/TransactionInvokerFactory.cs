using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data.TransactionParameters;
using System;

namespace Neo.UI.Core.Controllers
{
    public class TransactionInvokerFactory : ITransactionInvokerFactory
    {
        #region ITransactionInvokerFactory Implementation 
        public ITransactionInvoker GetTransactionInvoker(
            IWalletController walletController,
            InvocationTransactionType invocationTransactionType,
            AssetRegistrationTransactionParameters assetRegistrationParameters,
            AssetTransferTransactionParameters assetTransferTransactionParameters,
            DeployContractTransactionParameters deployContractTransactionParameters,
            ElectionTransactionParameters electionTransactionParameters,
            VotingTransactionParameters votingTransactionParameters)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
