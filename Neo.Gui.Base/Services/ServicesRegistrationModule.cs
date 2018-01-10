using Autofac;

using Neo.Gui.Base.Services.Implementations;
using Neo.Gui.Base.Services.Interfaces;

namespace Neo.Gui.Base.Services
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