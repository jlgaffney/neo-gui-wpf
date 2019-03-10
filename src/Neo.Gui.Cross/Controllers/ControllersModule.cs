using Autofac;

namespace Neo.Gui.Cross.Controllers
{
    public class ControllersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ApplicationController>().AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
