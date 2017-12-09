using Autofac;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Services;
using Neo.Gui.Wpf.Implementations.Helpers;
using Neo.Gui.Wpf.Implementations.Managers;
using Neo.Gui.Wpf.Implementations.Services;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class HelpersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ClipboardManager>()
                .As<IClipboardManager>()
                .SingleInstance();

            builder
                .RegisterType<DialogManager>()
                .As<IDialogManager>()
                .SingleInstance();

            builder
                .RegisterType<DispatchService>()
                .As<IDispatchService>()
                .SingleInstance();

            builder
                .RegisterType<NotificationService>()
                .As<INotificationService>()
                .SingleInstance();

            builder
                .RegisterType<ProcessHelper>()
                .As<IProcessHelper>()
                .SingleInstance();

            builder
                .RegisterType<SettingsManager>()
                .As<ISettingsManager>()
                .SingleInstance();

            builder
                .RegisterType<ThemeManager>()
                .As<IThemeManager>()
                .SingleInstance();

            builder
                .RegisterType<VersionHelper>()
                .As<IVersionHelper>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
