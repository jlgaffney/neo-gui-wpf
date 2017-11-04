using Autofac;
using Neo.UI.Base.MVVM;
using Neo.UI.Home;
using Neo.UI.Updater;

namespace Neo.UI
{
    public class ViewModelsRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<UpdateViewModel>()
                .As<ViewModelBase>()
                .WithParameter("ViewModel", "UpdateViewModel");
            builder
                .RegisterType<HomeViewModel>()
                .As<ViewModelBase>()
                .WithParameter("ViewModel", "HomeViewModel");

            // TODO Register view model types in loaded assemblies based on naming convention to making adding new view model classes easier
            // e.g. automatically register loaded types named "QwertyViewModel" or "AbcdefViewModel", etc.

            base.Load(builder);
        }
    }
}
