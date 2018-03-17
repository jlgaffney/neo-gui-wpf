using Autofac;
using Neo.UI.Core.Internal.Services.Implementations;
using Neo.UI.Core.Internal.Services.Interfaces;

namespace Neo.UI.Core.Internal.Services
{
    public class ServicesRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<BlockchainImportService>()
                .As<IBlockchainImportService>()
                .SingleInstance();

            builder
                .RegisterType<CertificateQueryService>()
                .As<ICertificateQueryService>()
                .SingleInstance();

            builder
                .RegisterType<BlockchainService>()
                .As<IBlockchainService>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}