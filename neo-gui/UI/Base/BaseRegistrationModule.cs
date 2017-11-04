using Autofac;
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
                .RegisterType<MessageAggregator>()
                .As<IMessageAggregator>()
                .SingleInstance();

            // Register thread dispatcher
            builder
                .RegisterType<Dispatcher>()
                .As<IDispatcher>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
