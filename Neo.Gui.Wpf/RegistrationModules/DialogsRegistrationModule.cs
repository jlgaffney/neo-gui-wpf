using Autofac;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Contracts;
using Neo.Gui.Base.Dialogs.Results.Development;
using Neo.Gui.Base.Dialogs.Results.Home;
using Neo.Gui.Base.Dialogs.Results.Settings;
using Neo.Gui.Base.Dialogs.Results.Voting;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Wpf.Views;
using Neo.Gui.Wpf.Views.Accounts;
using Neo.Gui.Wpf.Views.Assets;
using Neo.Gui.Wpf.Views.Contracts;
using Neo.Gui.Wpf.Views.Development;
using Neo.Gui.Wpf.Views.Home;
using Neo.Gui.Wpf.Views.Settings;
using Neo.Gui.Wpf.Views.Transactions;
using Neo.Gui.Wpf.Views.Updater;
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
            RegisterHomeDialogs(builder);
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
                .RegisterType<ContractParametersEditorView>()
                .As<IDialog<ContractParametersEditorDialogResult>>();
            builder
                .RegisterType<DeployContractView>()
                .As<IDialog<DeployContractDialogResult>>();
            builder
                .RegisterType<InvokeContractView>()
                .As<IDialog<InvokeContractDialogResult>>();
        }

        private static void RegisterHomeDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<HomeView>()
                .As<IDialog<HomeDialogResult>>();
        }

        private static void RegisterSettingsDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<SettingsView>()
                .As<IDialog<SettingsDialogResult>>();
            builder
                .RegisterType<UpdateView>()
                .As<IDialog<UpdateDialogResult>>();
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
            builder
                .RegisterType<VotingView>()
                .As<IDialog<VotingDialogResult>>();
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
                .RegisterType<ClaimView>()
                .As<IDialog<ClaimDialogResult>>();
            builder
                .RegisterType<CreateLockAccountView>()
                .As<IDialog<CreateLockAccountDialogResult>>();
            builder
                .RegisterType<CreateMultiSigContractView>()
                .As<IDialog<CreateMultiSigContractDialogResult>>();
            builder
                .RegisterType<CreateWalletView>()
                .As<IDialog<CreateWalletDialogResult>>();
            builder
                .RegisterType<ImportCertificateView>()
                .As<IDialog<ImportCertificateDialogResult>>();
            builder
                .RegisterType<ImportCustomContractView>()
                .As<IDialog<ImportCustomContractDialogResult>>();
            builder
                .RegisterType<ImportPrivateKeyView>()
                .As<IDialog<ImportPrivateKeyDialogResult>>();
            builder
                .RegisterType<OpenWalletView>()
                .As<IDialog<OpenWalletDialogResult>>();
            builder
                .RegisterType<TransferView>()
                .As<IDialog<TransferDialogResult>>();
            builder
                .RegisterType<TradeVerificationView>()
                .As<IDialog<TradeVerificationDialogResult>>();
            builder
                .RegisterType<TradeView>()
                .As<IDialog<TradeDialogResult>>();
            builder
                .RegisterType<ViewContractView>()
                .As<IDialog<ViewContractDialogResult>>();
            builder
                .RegisterType<ViewPrivateKeyView>()
                .As<IDialog<ViewPrivateKeyDialogResult>>();
            builder
                .RegisterType<AboutView>()
                .As<IDialog<AboutDialogResult>>();
            builder
                .RegisterType<DeveloperToolsView>()
                .As<IDialog<DeveloperToolsDialogResult>>();
        }
    }
}
