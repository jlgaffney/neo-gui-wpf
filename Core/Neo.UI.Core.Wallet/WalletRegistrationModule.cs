using Autofac;

using Neo.UI.Core.Messaging;
using Neo.UI.Core.Services.Implementations;
using Neo.UI.Core.Transactions;

namespace Neo.UI.Core.Wallet
{
    public class WalletRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<WalletController>()
                .As<IWalletController>()
                .SingleInstance();

            // Register modules
            builder.RegisterModule<MessagingRegistrationModule>();
            builder.RegisterModule<ServicesRegistrationModule>();
            builder.RegisterModule<TransactionsRegistrationModule>();

            base.Load(builder);
        }
    }
}
