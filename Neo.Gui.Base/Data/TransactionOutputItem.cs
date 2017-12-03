using Neo.Wallets;

namespace Neo.Gui.Base.Data
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