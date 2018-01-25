using System;

using System.Linq;
using System.Text;

using Neo.Core;
using Neo.IO.Json;
using Neo.SmartContract;
using Neo.VM;

using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.UI.Core.Controllers.TransactionInvokers
{
    internal abstract class TransactionInvokerBase : ITransactionInvoker
    {
        #region Public Properties 
        public InvocationTransaction Transaction { get; set; }
        #endregion

        #region ITransactionInvoker Implementation 
        public ITransactionConfiguration Configuration { get; set; }

        public bool IsContractTransaction { get; set; }

        public abstract bool IsValid(InvocationTransactionType invocationTransactionType);

        public abstract void GenerateTransaction();

        public string GetTransactionScript()
        {
            return this.Transaction.Script.ToHexString();
        }

        public void Invoke()
        {
            if (this.Transaction == null)
            {
                if (this.IsContractTransaction)
                {
                    throw new InvalidOperationException("Cannot invoke an InvocationTransaction before create it.");
                }
                else
                {
                    throw new InvalidOperationException("The transaction is not InvocationTransaction to be Invoked. Please use the SignAndRelayTransaction method.");
                }
            }

            this.Configuration.WalletController.InvokeContract(this.Transaction);
        }

        public TestForGasUsageResult TestForGasUsage(string customScript)
        {
            if (this.Transaction == null)
            {
                if (this.IsContractTransaction)
                {
                    throw new InvalidOperationException("Cannot invoke an InvocationTransaction before create it.");
                }
                else
                {
                    throw new InvalidOperationException("The transaction is not InvocationTransaction to be Invoked. Please use the SignAndRelayTransaction method.");
                }
            }

            var transactionExecutionFailed = false;
            var transactionFee = Fixed8.Zero;

            this.Transaction.Version = 1;
            this.Transaction.Script = customScript.Trim().HexToBytes();

            // Load default transaction values if required
            if (this.Transaction.Attributes == null) this.Transaction.Attributes = new TransactionAttribute[0];
            if (this.Transaction.Inputs == null) this.Transaction.Inputs = new CoinReference[0];
            if (this.Transaction.Outputs == null) this.Transaction.Outputs = new TransactionOutput[0];
            if (this.Transaction.Scripts == null) this.Transaction.Scripts = new Witness[0];

            var engine = ApplicationEngine.Run(this.Transaction.Script, this.Transaction);

            // Get transaction test results
            var builder = new StringBuilder();
            builder.AppendLine($"VM State: {engine.State}");
            builder.AppendLine($"Gas Consumed: {engine.GasConsumed}");
            builder.AppendLine($"Evaluation Stack: {new JArray(engine.EvaluationStack.Select(p => p.ToParameter().ToJson()))}");

            if (!engine.State.HasFlag(VMState.FAULT))
            {
                this.Transaction.Gas = engine.GasConsumed - Fixed8.FromDecimal(10);

                if (this.Transaction.Gas < Fixed8.Zero) this.Transaction.Gas = Fixed8.Zero;

                this.Transaction.Gas = this.Transaction.Gas.Ceiling();

                transactionFee = this.Transaction.Gas.Equals(Fixed8.Zero) ? this.Configuration.WalletController.NetworkFee : this.Transaction.Gas;
            }
            else
            {
                transactionExecutionFailed = true;
            }

            return new TestForGasUsageResult(builder.ToString(), transactionFee.ToString(), transactionExecutionFailed);
        }

        public virtual void SignAndRelayTransaction()
        {
            // Not all transactions type will override method
            throw new InvalidOperationException("The SignAndRelayTransaction need to be overwriten in the Invoker.");
        }
        #endregion
    }
}
