using Autofac;

using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Wpf.Implementations.Managers;
using Neo.Gui.Wpf.Implementations.Services;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class HelpersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ProcessManager>()
                .As<IProcessManager>()
                .SingleInstance();

            builder
                .RegisterType<VersionService>()
                .As<IVersionService>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
