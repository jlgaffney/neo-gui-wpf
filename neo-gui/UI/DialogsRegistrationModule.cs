using Autofac;
using Neo.DialogResults;
using Neo.Gui.Helpers.Interfaces;
using Neo.UI.Wallets;

namespace Neo.UI
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
