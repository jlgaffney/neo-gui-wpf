using Autofac;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Wpf.DialogResults;
using Neo.Gui.Wpf.Views.Wallets;

namespace Neo.Gui.Wpf.Views
{
    public class DialogsRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<OpenWalletView>()
                .As<IDialog<OpenWalletDialogResult>>();

            base.Load(builder);
        }
    }
}
