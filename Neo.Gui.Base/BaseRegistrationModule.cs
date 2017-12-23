using Autofac;
using Neo.Gui.Base.Certificates;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Messaging;

namespace Neo.Gui.Base
{
    public class BaseRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register modules
            builder.RegisterModule<ControllersRegistrationModule>();
            builder.RegisterModule<MessagingRegistrationModule>();

            // Register types
            builder
                .RegisterType<CertificateService>()
                .As<ICertificateService>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
