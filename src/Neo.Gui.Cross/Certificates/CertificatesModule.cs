using Autofac;

namespace Neo.Gui.Cross.Certificates
{
    public class CertificatesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CertificateQueryService>().AsImplementedInterfaces();
        }
    }
}
