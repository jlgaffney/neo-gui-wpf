using Autofac;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Dialogs.LoadParameters.Assets;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Dialogs.LoadParameters.Development;
using Neo.Gui.Dialogs.LoadParameters.Home;
using Neo.Gui.Dialogs.LoadParameters.Settings;
using Neo.Gui.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Dialogs.LoadParameters.Updater;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.Gui.Dialogs.LoadParameters.Wallets;

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
                .As<IDialog<AssetDistributionLoadParameters>>();
            builder
                .RegisterType<AssetRegistrationView>()
                .As<IDialog<AssetRegistrationLoadParameters>>();
        }

        private static void RegisterContractDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<ContractParametersEditorView>()
                .As<IDialog<ContractParametersEditorLoadParameters>>();
            builder
                .RegisterType<DeployContractView>()
                .As<IDialog<DeployContractLoadParameters>>();
            builder
                .RegisterType<InvokeContractView>()
                .As<IDialog<InvokeContractLoadParameters>>();
        }

        private static void RegisterHomeDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<HomeView>()
                .As<IDialog<HomeLoadParameters>>();
        }

        private static void RegisterSettingsDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<SettingsView>()
                .As<IDialog<SettingsLoadParameters>>();
            builder
                .RegisterType<UpdateView>()
                .As<IDialog<UpdateLoadParameters>>();
        }

        private static void RegisterTransactionDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<BulkPayView>()
                .As<IDialog<BulkPayLoadParameters>>();
            builder
                .RegisterType<PayToView>()
                .As<IDialog<PayToLoadParameters>>();
            builder
                .RegisterType<SigningView>()
                .As<IDialog<SigningLoadParameters>>();
        }

        private static void RegisterVotingDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<ElectionView>()
                .As<IDialog<ElectionLoadParameters>>();
            builder
                .RegisterType<VotingView>()
                .As<IDialog<VotingLoadParameters>>();
        }

        private static void RegisterWalletDialogs(ContainerBuilder builder)
        {
            builder
                .RegisterType<CertificateApplicationView>()
                .As<IDialog<CertificateApplicationLoadParameters>>();
            builder
                .RegisterType<ClaimView>()
                .As<IDialog<ClaimLoadParameters>>();
            builder
                .RegisterType<CreateLockAccountView>()
                .As<IDialog<CreateLockAccountLoadParameters>>();
            builder
                .RegisterType<CreateMultiSigContractView>()
                .As<IDialog<CreateMultiSigContractLoadParameters>>();
            builder
                .RegisterType<CreateWalletView>()
                .As<IDialog<CreateWalletLoadParameters>>();
            builder
                .RegisterType<ImportCertificateView>()
                .As<IDialog<ImportCertificateLoadParameters>>();
            builder
                .RegisterType<ImportCustomContractView>()
                .As<IDialog<ImportCustomContractLoadParameters>>();
            builder
                .RegisterType<ImportPrivateKeyView>()
                .As<IDialog<ImportPrivateKeyLoadParameters>>();
            builder
                .RegisterType<OpenWalletView>()
                .As<IDialog<OpenWalletLoadParameters>>();
            builder
                .RegisterType<TransferView>()
                .As<IDialog<TransferLoadParameters>>();
            builder
                .RegisterType<TradeVerificationView>()
                .As<IDialog<TradeVerificationLoadParameters>>();
            builder
                .RegisterType<TradeView>()
                .As<IDialog<TradeLoadParameters>>();
            builder
                .RegisterType<ViewContractView>()
                .As<IDialog<ViewContractLoadParameters>>();
            builder
                .RegisterType<ViewPrivateKeyView>()
                .As<IDialog<ViewPrivateKeyLoadParameters>>();
            builder
                .RegisterType<AboutView>()
                .As<IDialog<AboutLoadParameters>>();
            builder
                .RegisterType<DeveloperToolsView>()
                .As<IDialog<DeveloperToolsLoadParameters>>();
        }
    }
}
