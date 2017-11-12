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

            base.Load(builder);
        }
    }
}
