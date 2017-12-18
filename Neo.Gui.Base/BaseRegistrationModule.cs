using Autofac;
using Neo.Gui.Base.Certificates;

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
