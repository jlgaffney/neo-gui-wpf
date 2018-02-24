using Autofac;

using Neo.UI.Core.Transactions.Builders;
using Neo.UI.Core.Transactions.Implementations;
using Neo.UI.Core.Transactions.Interfaces;

namespace Neo.UI.Core.Transactions
{
    public class TransactionsRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<TransactionBuilderFactory>()
                .As<ITransactionBuilderFactory>();


            // Register builders
            builder
                .RegisterType<AssetDistributionTransactionBuilder>()
                .As<ITransactionBuilder>();

            builder
                .RegisterType<AssetRegistrationTransactionBuilder>()
                .As<ITransactionBuilder>();

            builder
                .RegisterType<AssetTransferTransactionBuilder>()
                .As<ITransactionBuilder>();

            builder
                .RegisterType<DeployContractTransactionBuilder>()
                .As<ITransactionBuilder>();

            builder
                .RegisterType<ValidatorRegisterTransactionBuilder>()
                .As<ITransactionBuilder>();

            builder
                .RegisterType<VotingTransactionBuilder>()
                .As<ITransactionBuilder>();

            builder
                .RegisterType<InvokeContractTransactionBuilder>()
                .As<ITransactionBuilder>();

            base.Load(builder);
        }
    }
}
