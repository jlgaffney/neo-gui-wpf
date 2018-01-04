using Autofac;

using Neo.Gui.Base.Messaging.Implementations;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.Base.Messaging
{
    internal class MessagingRegistrationModule : Module
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