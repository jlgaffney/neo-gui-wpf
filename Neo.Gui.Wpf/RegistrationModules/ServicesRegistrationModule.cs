using Autofac;
using Neo.Gui.Base.Services;
using Neo.Gui.Wpf.Implementations.Services;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class ServicesRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<DispatchService>()
                .As<IDispatchService>()
                .SingleInstance();

            builder
                .RegisterType<FileDialogService>()
                .As<IFileDialogService>()
                .SingleInstance();

            builder
                .RegisterType<NotificationService>()
                .As<INotificationService>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
