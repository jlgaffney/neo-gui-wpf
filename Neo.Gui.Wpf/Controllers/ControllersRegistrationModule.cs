using Autofac;
using Neo.Gui.Wpf.Properties;

namespace Neo.Controllers
{
    public class ControllersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var blockChainControllerType = Settings.Default.RemoteNodeMode
                ? typeof(RemoteBlockChainController)
                : typeof(LocalBlockChainController);

            builder
                .RegisterType(blockChainControllerType)
                .As<IBlockChainController>()
                .SingleInstance();

            builder
                .RegisterType<WalletController>()
                .As<IWalletController>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
