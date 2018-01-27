using Neo.Core;
using Neo.UI.Core.Data.TransactionParameters;

namespace Neo.UI.Core.Transactions.Builders
{
    internal class AssetTransferTransactionBuilder : TransactionBuilderBase
    {
        private Transaction nonContractTransaction;

        public override bool IsValid(InvocationTransactionType invocationTransactionType)
        {
            return invocationTransactionType == InvocationTransactionType.AssetTransfer;
        }

        public override void GenerateTransaction()
        {
            var assetTransferTransactionConfiguration = this.Configuration as AssetTransferTransactionConfiguration;
            var parameters = assetTransferTransactionConfiguration.AssetTransferTransactionParameters;

            var transferFee = Fixed8.Parse(parameters.TransferFee);

            UInt160 transferChangeAddress = null;
            if (!string.IsNullOrEmpty(parameters.TransferChangeAddress))
            {
                transferChangeAddress = this.Configuration.WalletController.AddressToScriptHash(parameters.TransferChangeAddress);
            }

            var transaction =  this.Configuration.WalletController.MakeTransferTransaction(parameters.TransactionOutputItems, parameters.Remark, transferChangeAddress, transferFee);

            if (transaction is InvocationTransaction)
            {
                this.Transaction = transaction as InvocationTransaction;
                this.IsContractTransaction = false;
            }
            else
            {
                this.nonContractTransaction = transaction;
                this.IsContractTransaction = true;
            }
        }

        public override void SignAndRelayTransaction()
        {
            this.Configuration.WalletController.SignAndRelay(this.nonContractTransaction);
        }
    }
}
