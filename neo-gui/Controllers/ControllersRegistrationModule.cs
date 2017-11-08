using Autofac;

namespace Neo.Controllers
{
    public class ControllersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<BlockChainController>()
                .As<IBlockChainController>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
