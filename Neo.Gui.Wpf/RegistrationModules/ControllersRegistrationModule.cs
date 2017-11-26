using Autofac;
using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.Wpf.Controllers;
using Neo.Gui.Wpf.Properties;

namespace Neo.Gui.Wpf.RegistrationModules
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
