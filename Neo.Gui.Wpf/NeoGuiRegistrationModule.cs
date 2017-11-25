using Autofac;

namespace Neo.Gui.Wpf
{
    public class NeoGuiRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ApplicationContext>()
                .As<IApplicationContext>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
