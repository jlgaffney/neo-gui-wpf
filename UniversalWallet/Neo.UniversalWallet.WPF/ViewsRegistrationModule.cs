using Autofac;
using Neo.UniversalWallet.ViewModels.Helpers;
using Neo.UniversalWallet.WPF.Views;
using Module = Autofac.Module;

namespace Neo.UniversalWallet.WPF
{
    public class ViewsRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<LoadWalletView>()
                .Keyed<IView>("LoadWalletView");

            builder
                .RegisterType<DashboardView>()
                .Keyed<IView>("DashboardView");

            builder
                .RegisterType<AssetView>()
                .Keyed<IView>("AssetView");

            base.Load(builder);
        }
    }
}
