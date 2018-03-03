using Autofac;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Internal.Services.Implementations
{
    public class ServicesRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // TODO Implement a way of switching between remote and local blockchain controllers
            const bool lightMode = false;
            var blockChainControllerType = lightMode
                ? typeof(RemoteBlockchainService)
                : typeof(LocalBlockchainService);

            builder
                .RegisterType(blockChainControllerType)
                .As<IBlockchainService>()
                .SingleInstance();

            

            builder
                .RegisterType<BlockchainImportService>()
                .As<IBlockchainImportService>();

            builder
                .RegisterType<CertificateQueryService>()
                .As<ICertificateQueryService>();

            base.Load(builder);
        }
    }
}