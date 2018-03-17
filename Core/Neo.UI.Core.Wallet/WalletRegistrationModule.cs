using Autofac;

using Neo.UI.Core.Messaging;
using Neo.UI.Core.Services.Implementations;
using Neo.UI.Core.Transactions;
using Neo.UI.Core.Wallet.Implementations;
using InternalServicesRegistrationModule = Neo.UI.Core.Internal.Services.ServicesRegistrationModule;

namespace Neo.UI.Core.Wallet
{
    public class WalletRegistrationModule : Module
    {
        /// <summary>
        /// Set this flag before registering the module to enable light wallet mode.
        /// </summary>
        public static bool LightMode;

        protected override void Load(ContainerBuilder builder)
        {
            var walletControllerType = LightMode
                ? typeof(LightWalletController)
                : typeof(FullWalletController);
            builder
                .RegisterType(walletControllerType)
                .As<IWalletController>()
                .SingleInstance();

            // Register modules
            builder.RegisterModule<InternalServicesRegistrationModule>();
            builder.RegisterModule<MessagingRegistrationModule>();
            builder.RegisterModule<ServicesRegistrationModule>();
            builder.RegisterModule<TransactionsRegistrationModule>();

            base.Load(builder);
        }
    }
}
