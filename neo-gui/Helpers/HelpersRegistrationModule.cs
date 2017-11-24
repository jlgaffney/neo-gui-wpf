using Autofac;
using Neo.Gui.Helpers.Interfaces;

namespace Neo.Helpers
{
    public class HelpersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<DialogHelper>()
                .As<IDialogHelper>();

            builder
                .RegisterType<ExternalProcessHelper>()
                .As<IExternalProcessHelper>();

            builder
                .RegisterType<NotificationHelper>()
                .As<INotificationHelper>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
