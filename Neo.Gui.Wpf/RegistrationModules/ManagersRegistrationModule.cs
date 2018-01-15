using Autofac;

using Neo.Gui.Base.Managers.Interfaces;

using Neo.Gui.Wpf.Implementations.Managers;
using Neo.UI.Core.Managers.Interfaces;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class ManagersRegistrationModule : Module
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
                .RegisterType<SettingsManager>()
                .As<ISettingsManager>()
                .SingleInstance();

            builder
                .RegisterType<ThemeManager>()
                .As<IThemeManager>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
