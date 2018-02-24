using Autofac;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Wpf.Native.Services;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.Gui.Wpf.Native
{
    public class NativeServicesRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CertificateRequestService>()
                .As<ICertificateRequestService>()
                .SingleInstance();

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
                .RegisterType<FileDialogService>()
                .As<IFileDialogService>()
                .SingleInstance();

            builder
                .RegisterType<NotificationService>()
                .As<INotificationService>()
                .SingleInstance();

            builder
                .RegisterType<ProcessManager>()
                .As<IProcessManager>()
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
                .RegisterType<VersionService>()
                .As<IVersionService>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
