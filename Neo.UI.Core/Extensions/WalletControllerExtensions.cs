//using Neo.UI.Core.Controllers;
//using Neo.UI.Core.Data.TransactionParameters;

//namespace Neo.UI.Core.Extensions
//{
//    public static class WalletControllerExtensions
//    {
//        public static ITransactionInvoker GetTransactionInvoker(
//            InvocationTransactionType invocationTransactionType,
//            AssetRegistrationTransactionParameters assetRegistrationTransactionParameters,
//            AssetTransferTransactionParameters assetTransferTransactionParameters,
//            DeployContractTransactionParameters deployContractTransactionParameters,
//            ElectionTransactionParameters electionTransactionParameters,
//            VotingTransactionParameters votingTransactionParameters)
//        {
//            var factory = new TransactionInvokerFactory(
//                invocationTransactionType,
//                assetRegistrationTransactionParameters,
//                assetTransferTransactionParameters,
//                deployContractTransactionParameters,
//                electionTransactionParameters,
//                votingTransactionParameters);

//            return factory.GetTransactionInvoker();
//        }
//    }
//}
