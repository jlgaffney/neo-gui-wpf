using System.Collections.Generic;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Transactions.Parameters
{
    public class AssetTransferTransactionParameters
    {
        public IEnumerable<TransactionOutputItem> TransactionOutputItems { get; }

        public string Remark { get; }

        public string TransferChangeAddress { get; }

        public string TransferFee { get; }

        public AssetTransferTransactionParameters(IEnumerable<TransactionOutputItem> transactionOutputItems, string transferChangeAddress, string remark, string transferFee)
        {
            this.TransactionOutputItems = transactionOutputItems;
            this.TransferChangeAddress = transferChangeAddress;
            this.Remark = remark;
            this.TransferFee = transferFee;
        }
    }
}
