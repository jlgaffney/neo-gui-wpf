using Autofac;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Wpf.Views.Assets;
using Neo.Gui.Wpf.Views.Contracts;
using Neo.Gui.Wpf.Views.Settings;
using Neo.Gui.Wpf.Views.Transactions;
using Neo.Gui.Wpf.Views.Voting;
using Neo.Gui.Wpf.Views.Wallets;

namespace Neo.Gui.Wpf.RegistrationModules
{
    public class DialogsRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterAssetDialogs(builder);
            RegisterContractDialogs(builder);
            RegisterSettingsDialogs(builder);
            RegisterTransactionDialogs(builder);
            RegisterVotingDialogs(builder);
            RegisterWalletDialogs(builder);

            base.Load(builder);
        }

        private static void RegisterAssetDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<AssetDistributionView>()
                .As<IDialog<AssetDistributionDialogResult>>();
            builder
                .RegisterType<AssetRegistrationView>()
                .As<IDialog<AssetRegistrationDialogResult>>();
        }

        private static void RegisterContractDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<DeployContractView>()
                .As<IDialog<DeployContractDialogResult>>();
        }

        private static void RegisterSettingsDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<SettingsView>()
                .As<IDialog<SettingsDialogResult>>();
        }

        private static void RegisterTransactionDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<SigningView>()
                .As<IDialog<SigningDialogResult>>();
        }

        private static void RegisterVotingDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<ElectionView>()
                .As<IDialog<ElectionDialogResult>>();
        }

        private static void RegisterWalletDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<CertificateApplicationView>()
                .As<IDialog<CertificateApplicationDialogResult>>();
            builder
                .RegisterType<ChangePasswordView>()
                .As<IDialog<ChangePasswordDialogResult>>();
            builder
                .RegisterType<CreateWalletView>()
                .As<IDialog<CreateWalletDialogResult>>();
            builder
                .RegisterType<OpenWalletView>()
                .As<IDialog<OpenWalletDialogResult>>();
            builder
                .RegisterType<RestoreAccountsView>()
                .As<IDialog<RestoreAccountsDialogResult>>();
            builder
                .RegisterType<TransferView>()
                .As<IDialog<TransferDialogResult>>();
            builder
                .RegisterType<TradeView>()
                .As<IDialog<TradeDialogResult>>();
        }
    }
}
