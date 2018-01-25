using System.Collections.Generic;

namespace Neo.UI.Core.Data.TransactionParameters
{
    public class AssetTransferTransactionParameters
    {
        public IEnumerable<TransactionOutputItem> TransactionOutputItems { get; private set; }

        public string Remark { get; private set; }

        public string TransferChangeAddress { get; private set; }

        public string TransferFee { get; private set; }

        public AssetTransferTransactionParameters(IEnumerable<TransactionOutputItem> transactionOutputItems, string transferChangeAddress, string remark, string transferFee)
        {
            this.TransactionOutputItems = transactionOutputItems;
            this.TransferChangeAddress = transferChangeAddress;
            this.Remark = remark;
            this.TransferFee = transferFee;
        }
    }
}
