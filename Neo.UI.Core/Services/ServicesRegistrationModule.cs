using Autofac;
using Neo.UI.Core.Services.Implementations;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Services
{
    internal class ServicesRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CertificateService>()
                .As<ICertificateService>();

            builder
                .RegisterType<StoreCertificateService>()
                .As<IStoreCertificateService>();

            base.Load(builder);
        }
    }
}