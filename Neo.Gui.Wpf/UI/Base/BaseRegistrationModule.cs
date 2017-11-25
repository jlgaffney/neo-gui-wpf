using Autofac;
using Neo.Gui.Base.Interfaces.Helpers;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.Messages;

namespace Neo.UI.Base
{
    public class BaseRegistrationModule : Module
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