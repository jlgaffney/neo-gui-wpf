using System;
using System.Collections.Generic;
using System.Linq;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data.TransactionParameters;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;

namespace Neo.UI.Core.Transactions.Implementations
{
    internal class TransactionBuilderFactory : ITransactionBuilderFactory
    {
        #region ITransactionBuilderFactory Implementation 
        public ITransactionBuilder GetTransactionInvoker(
            IWalletController walletController,
            IEnumerable<ITransactionBuilder> transactionInvokers,
            InvocationTransactionType invocationTransactionType,
            AssetRegistrationTransactionParameters assetRegistrationParameters,
            AssetTransferTransactionParameters assetTransferTransactionParameters,
            DeployContractTransactionParameters deployContractTransactionParameters,
            ElectionTransactionParameters electionTransactionParameters,
            VotingTransactionParameters votingTransactionParameters)
        {
            var configDictionary = new Dictionary<InvocationTransactionType, ITransactionConfiguration>
            {
                { InvocationTransactionType.AssetRegistration, new AssetRegistrationTransactionConfiguration { WalletController = walletController, InvocationTransactionType = invocationTransactionType, AssetRegistrationTransactionParameters = assetRegistrationParameters }},
                { InvocationTransactionType.AssetTransfer, new AssetTransferTransactionConfiguration { WalletController = walletController, InvocationTransactionType = invocationTransactionType, AssetTransferTransactionParameters = assetTransferTransactionParameters }},
                { InvocationTransactionType.DeployContract, new DeployContractTransactionConfiguration { WalletController = walletController, InvocationTransactionType = invocationTransactionType, DeployContractTransactionParameters = deployContractTransactionParameters }},
                { InvocationTransactionType.Election, new ElectionTransactionConfiguration { WalletController = walletController, InvocationTransactionType = invocationTransactionType, ElectionTransactionParameters = electionTransactionParameters}},
                { InvocationTransactionType.Vote, new VotingTransactionConfiguration { WalletController = walletController, InvocationTransactionType = invocationTransactionType, VotingTransactionParameters = votingTransactionParameters}}
            };

            try
            {
                var invoker = transactionInvokers.Single(x => x.IsValid(invocationTransactionType));

                invoker.Configuration = configDictionary[invocationTransactionType];
                invoker.GenerateTransaction();
                return invoker;
            }
            catch 
            {
                throw new InvalidOperationException($"Strategy for {invocationTransactionType.ToString()} not found.");
            }
        }
        #endregion
    }
}
