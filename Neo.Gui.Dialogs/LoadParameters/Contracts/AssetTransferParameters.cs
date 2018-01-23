using Neo.UI.Core.Data;
using System.Collections.Generic;

namespace Neo.Gui.Dialogs.LoadParameters.Contracts
{
    public class AssetTransferParameters
    {
        public IEnumerable<TransactionOutputItem> TransactionOutputItems { get; private set; }

        public string Remark { get; private set; }

        public string TransferChangeAddress { get; private set; }

        public string TransferFee { get; private set; }

        public AssetTransferParameters(IEnumerable<TransactionOutputItem> transactionOutputItems, string transferChangeAddress, string remark, string transferFee)
        {
            this.TransactionOutputItems = transactionOutputItems;
            this.TransferChangeAddress = transferChangeAddress;
            this.Remark = remark;
            this.TransferFee = transferFee;
        }
    }
}
