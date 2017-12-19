using Autofac;
using Neo.Gui.Base.Helpers;
using Neo.Gui.Wpf.Implementations.Helpers;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class HelpersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ProcessHelper>()
                .As<IProcessHelper>()
                .SingleInstance();

            builder
                .RegisterType<VersionHelper>()
                .As<IVersionHelper>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
