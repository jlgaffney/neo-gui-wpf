using System;
using System.Linq;
using System.Text;
using Neo.Core;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.UI.Core.Transactions.Builders;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Testing;
using Neo.VM;

namespace Neo.UI.Core.Transactions.Implementations
{
    internal class TransactionTester : ITransactionTester
    {
        public TestForGasUsageResult TestForGasUsage(ITransactionBuilder builder, string customScript)
        {
            var builderBase = builder as TransactionBuilderBase;

            if (builderBase.Transaction == null)
            {
                if (builderBase.IsContractTransaction)
                {
                    throw new InvalidOperationException("Cannot invoke an InvocationTransaction before create it.");
                }
                else
                {
                    throw new InvalidOperationException(
                        "The transaction is not InvocationTransaction to be Invoked. Please use the SignAndRelayTransaction method.");
                }
            }

            var transactionExecutionFailed = false;
            var transactionFee = Fixed8.Zero;

            builderBase.Transaction.Version = 1;
            builderBase.Transaction.Script = customScript.Trim().HexToBytes();

            // Load default transaction values if required
            if (builderBase.Transaction.Attributes == null) builderBase.Transaction.Attributes = new TransactionAttribute[0];
            if (builderBase.Transaction.Inputs == null) builderBase.Transaction.Inputs = new CoinReference[0];
            if (builderBase.Transaction.Outputs == null) builderBase.Transaction.Outputs = new TransactionOutput[0];
            if (builderBase.Transaction.Scripts == null) builderBase.Transaction.Scripts = new Witness[0];

            var engine = ApplicationEngine.Run(builderBase.Transaction.Script, builderBase.Transaction);

            // Get transaction test results
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"VM State: {engine.State}");
            stringBuilder.AppendLine($"Gas Consumed: {engine.GasConsumed}");
            stringBuilder.AppendLine($"Evaluation Stack: {new JArray(engine.EvaluationStack.Select(p => p.ToParameter().ToJson()))}");

            if (!engine.State.HasFlag(VMState.FAULT))
            {
                builderBase.Transaction.Gas = engine.GasConsumed - Fixed8.FromDecimal(10);

                if (builderBase.Transaction.Gas < Fixed8.Zero) builderBase.Transaction.Gas = Fixed8.Zero;

                builderBase.Transaction.Gas = builderBase.Transaction.Gas.Ceiling();

                transactionFee = builderBase.Transaction.Gas.Equals(Fixed8.Zero)
                    ? builderBase.Configuration.WalletController.NetworkFee
                    : builderBase.Transaction.Gas;
            }
            else
            {
                transactionExecutionFailed = true;
            }

            return new TestForGasUsageResult(stringBuilder.ToString(), transactionFee.ToString(), transactionExecutionFailed);
        }
    }
}
