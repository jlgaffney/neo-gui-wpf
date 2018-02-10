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

            builder
                .RegisterType<TransactionTester>()
                .As<ITransactionTester>();

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
                .RegisterType<ElectionTransactionBuilder>()
                .As<ITransactionBuilder>();

            builder
                .RegisterType<VotingTransactionBuilder>()
                .As<ITransactionBuilder>();

            builder
                .RegisterType<InvokeTransactionBuilder>()
                .As<ITransactionBuilder>();

            base.Load(builder);
        }
    }
}
