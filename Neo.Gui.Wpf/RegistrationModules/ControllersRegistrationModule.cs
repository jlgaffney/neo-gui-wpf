using Autofac;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Wpf.Properties;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class ControllersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // TODO Figure out a better way of switching between remote and local node controllers
            var blockChainControllerType = Settings.Default.P2P.RemoteNodeMode
                ? typeof(RemoteBlockchainController)
                : typeof(LocalBlockchainController);

            builder
                .RegisterType(blockChainControllerType)
                .As<IBlockchainController>()
                .SingleInstance();

            builder
                .RegisterType<WalletController>()
                .As<IWalletController>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
