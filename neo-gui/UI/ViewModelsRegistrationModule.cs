using Autofac;
using Neo.UI.Base.MVVM;
using Neo.UI.Home;

namespace Neo.UI
{
    public class ViewModelsRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<HomeViewModel>()
                .As<ViewModelBase>()
                .WithParameter("ViewModel", "HomeViewModel");

            base.Load(builder);
        }
    }
}
