using Autofac;
using Neo.Gui.Cross.Certificates;
using Neo.Gui.Cross.Controllers;
using Neo.Gui.Cross.Messaging;
using Neo.Gui.Cross.Services;
using Neo.Gui.Cross.ViewModels;

namespace Neo.Gui.Cross
{
    public class AppModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<CertificatesModule>();
            builder.RegisterModule<ControllersModule>();
            builder.RegisterModule<MessagingModule>();
            builder.RegisterModule<ServicesModule>();
            builder.RegisterModule<ViewModelModule>();
        }
    }
}
