using Autofac;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messaging;
using Neo.Gui.Base.Services;

namespace Neo.Gui.Base
{
    public class BaseRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register modules
            builder.RegisterModule<ControllersRegistrationModule>();
            builder.RegisterModule<ManagersRegistrationModule>();
            builder.RegisterModule<MessagingRegistrationModule>();
            builder.RegisterModule<ServicesRegistrationModule>();

            base.Load(builder);
        }
    }
}
