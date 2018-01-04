using Autofac;

using Neo.Gui.Base.Controllers.Implementations;
using Neo.Gui.Base.Controllers.Interfaces;

namespace Neo.Gui.Base.Controllers
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

            base.Load(builder);
        }
    }
}
