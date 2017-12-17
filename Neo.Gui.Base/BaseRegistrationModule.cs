using Autofac;
using Neo.Gui.Base.Certificates;
using Neo.Gui.Base.Services;

namespace Neo.Gui.Base
{
    public class BaseRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CertificateService>()
                .As<ICertificateService>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
