using Autofac;

namespace Neo.Gui.Cross.Messaging
{
    public class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MessageAggregator>().AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}
