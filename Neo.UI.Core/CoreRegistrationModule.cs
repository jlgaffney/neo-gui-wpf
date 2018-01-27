using Autofac;

using Neo.UI.Core.Controllers;
using Neo.UI.Core.Managers;
using Neo.UI.Core.Messaging;
using Neo.UI.Core.Services;
using Neo.UI.Core.Transactions;

namespace Neo.UI.Core
{
    public class CoreRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register modules
            builder.RegisterModule<ControllersRegistrationModule>();
            builder.RegisterModule<ManagersRegistrationModule>();
            builder.RegisterModule<MessagingRegistrationModule>();
            builder.RegisterModule<ServicesRegistrationModule>();
            builder.RegisterModule<TransactionsRegistrationModule>();

            base.Load(builder);
        }
    }
}
