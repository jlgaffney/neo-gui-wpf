using System.Collections.Generic;
using System.Linq;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Transactions.Parameters
{
    public class AssetTransferTransactionParameters : TransactionParameters
    {
        public IReadOnlyList<string> AccountScriptHashes { get; }

        public IReadOnlyList<TransactionOutputItem> TransactionOutputItems { get; }

        public string Remark { get; }

        public string TransferChangeAddress { get; }

        public string TransferFee { get; }

        public AssetTransferTransactionParameters(IEnumerable<string> accountScriptHashes, IEnumerable<TransactionOutputItem> transactionOutputItems, string transferChangeAddress, string remark, string transferFee)
        {
            this.AccountScriptHashes = accountScriptHashes.ToList();
            this.TransactionOutputItems = transactionOutputItems.ToList();
            this.TransferChangeAddress = transferChangeAddress;
            this.Remark = remark;
            this.TransferFee = transferFee;
        }
    }
}
