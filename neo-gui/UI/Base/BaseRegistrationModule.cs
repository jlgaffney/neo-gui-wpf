using Autofac;
using Neo.UI.Base.Messages;

namespace Neo.UI.Base
{
    public class BaseRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<MessageAggregator>()
                .As<IMessageAggregator>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
