using Autofac;
using Neo.UI.Core.Messaging.Implementations;
using Neo.UI.Core.Messaging.Interfaces;

namespace Neo.UI.Core.Messaging
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
                .As<IMessageSubscriber>()
                .SingleInstance();

            builder
                .RegisterType<MessagePublisher>()
                .As<IMessagePublisher>()
                .SingleInstance();
            

            base.Load(builder);
        }
    }
}