using Autofac;

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

            base.Load(builder);
        }
    }
}
