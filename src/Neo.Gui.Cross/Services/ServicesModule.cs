using Autofac;

namespace Neo.Gui.Cross.Services
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AccountBalanceService>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<AccountService>().AsImplementedInterfaces();
            builder.RegisterType<AssetService>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<BlockchainService>().AsImplementedInterfaces();
            builder.RegisterType<CertificateService>().AsImplementedInterfaces();
            builder.RegisterType<ClipboardService>().AsImplementedInterfaces();
            builder.RegisterType<ContractService>().AsImplementedInterfaces();
            builder.RegisterType<FileDialogService>().AsImplementedInterfaces();
            builder.RegisterType<NEP5TokenService>().AsImplementedInterfaces();
            builder.RegisterType<TransactionService>().AsImplementedInterfaces();
            builder.RegisterType<WalletService>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<WindowService>().AsImplementedInterfaces();
        }
    }
}
