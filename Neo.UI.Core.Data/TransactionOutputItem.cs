using Neo.Wallets;

namespace Neo.UI.Core.Data
{
    public class TransactionOutputItem : TransferOutput
    {
        public string AssetName;

        public override string ToString()
        {
            return $"{Wallet.ToAddress(ScriptHash)}\t{Value}\t{AssetName}";
        }
    }
}