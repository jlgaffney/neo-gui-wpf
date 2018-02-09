using System;
using Neo.Core;
using Neo.UI.Core.Data.TransactionParameters;
using Neo.UI.Core.Transactions.Interfaces;

namespace Neo.UI.Core.Transactions.Builders
{
    internal abstract class TransactionBuilderBase : ITransactionBuilder
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
            if (this.Transaction == null)
            {
                return string.Empty;
            }

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

        public virtual void SignAndRelayTransaction()
        {
            // Not all transactions type will override method
            throw new InvalidOperationException("The SignAndRelayTransaction need to be overwriten in the Invoker.");
        }
        #endregion
    }
}
