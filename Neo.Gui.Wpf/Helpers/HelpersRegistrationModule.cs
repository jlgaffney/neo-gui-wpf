using Autofac;
using Neo.Gui.Base.Helpers.Interfaces;

namespace Neo.Gui.Wpf.Helpers
{
    public class HelpersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<DialogHelper>()
                .As<IDialogHelper>();

            builder
                .RegisterType<DispatchHelper>()
                .As<IDispatchHelper>()
                .SingleInstance();

            builder
                .RegisterType<ExternalProcessHelper>()
                .As<IExternalProcessHelper>();

            builder
                .RegisterType<NotificationHelper>()
                .As<INotificationHelper>()
                .SingleInstance();

            builder
                .RegisterType<ThemeHelper>()
                .As<IThemeHelper>()
                .SingleInstance();

            builder
                .RegisterType<VersionHelper>()
                .As<IVersionHelper>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
