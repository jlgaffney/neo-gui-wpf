using Autofac;
using Neo.UI.Core.Managers.Implementations;
using Neo.UI.Core.Managers.Interfaces;

namespace Neo.UI.Core.Managers
{
    internal class ManagersRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CompressedFileManager>()
                .As<ICompressedFileManager>()
                .SingleInstance();

            builder
                .RegisterType<DirectoryManager>()
                .As<IDirectoryManager>()
                .SingleInstance();

            builder
                .RegisterType<FileManager>()
                .As<IFileManager>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
