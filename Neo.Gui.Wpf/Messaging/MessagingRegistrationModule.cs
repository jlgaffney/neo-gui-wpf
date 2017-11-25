using Autofac;
using Neo.Gui.Base.Messaging;

namespace Neo.Gui.Wpf.Messaging
{
    public class MessagingRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register message aggregator
            builder
                .RegisterType<InternalMessageAggregator>()
                .As<IInternalMessageAggregator>()
                .SingleInstance();

            builder
                .RegisterType<MessageSubscriber>()
                .As<IMessageSubscriber>();

            builder
                .RegisterType<MessagePublisher>()
                .As<IMessagePublisher>();
            

            base.Load(builder);
        }
    }
}