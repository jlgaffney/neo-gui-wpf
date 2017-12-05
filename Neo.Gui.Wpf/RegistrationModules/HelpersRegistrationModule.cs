using Autofac;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Wpf.Helpers;

namespace Neo.Gui.Wpf.RegistrationModules
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
                .RegisterType<ProcessHelper>()
                .As<IProcessHelper>();

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

            builder
                .RegisterType<SettingsHelper>()
                .As<ISettingsHelper>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
