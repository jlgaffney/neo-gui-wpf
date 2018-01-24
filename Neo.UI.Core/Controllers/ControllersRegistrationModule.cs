using Autofac;
using Neo.UI.Core.Controllers.Implementations;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Controllers.TransactionInvokers;

namespace Neo.UI.Core.Controllers
{
    internal class ControllersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // TODO Implement a way of switching between remote and local blockchain controllers
            const bool lightMode = false;
            var blockChainControllerType = lightMode
                ? typeof(RemoteBlockchainController)
                : typeof(LocalBlockchainController);

            builder
                .RegisterType(blockChainControllerType)
                .As<IBlockchainController>()
                .SingleInstance();

            builder
                .RegisterType<NetworkController>()
                .As<INetworkController>()
                .SingleInstance();

            builder
                .RegisterType<WalletController>()
                .As<IWalletController>()
                .SingleInstance();

            builder
                .RegisterType<TransactionInvokerFactory>()
                .As<ITransactionInvokerFactory>();

            builder
                .RegisterType<AssetRegistrationTransactionInvoker>()
                .As<ITransactionInvoker>();

            builder
                .RegisterType<AssetTransferTransactionInvoker>()
                .As<ITransactionInvoker>();

            builder
                .RegisterType<DeployContractTransactionInvoker>()
                .As<ITransactionInvoker>();

            base.Load(builder);
        }
    }
}
