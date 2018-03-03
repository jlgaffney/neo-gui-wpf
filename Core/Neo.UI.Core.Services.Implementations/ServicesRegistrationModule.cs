using Autofac;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Services.Implementations
{
    public class ServicesRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<DirectoryManager>()
                .As<IDirectoryManager>()
                .SingleInstance();

            builder
                .RegisterType<FileManager>()
                .As<IFileManager>()
                .SingleInstance();

            builder
                .RegisterType<StoreCertificateService>()
                .As<IStoreCertificateService>();

            base.Load(builder);
        }
    }
}