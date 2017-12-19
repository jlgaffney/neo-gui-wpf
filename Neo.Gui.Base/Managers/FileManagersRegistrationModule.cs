using Autofac;

namespace Neo.Gui.Base.Managers
{
    public class FileManagersRegistrationModule : Module
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
