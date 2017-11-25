using Neo.Wallets;

namespace Neo.UI
{
    public class TxOutListBoxItem : TransferOutput
    {
        public string AssetName;

        public override string ToString()
        {
            return $"{Wallet.ToAddress(ScriptHash)}\t{Value}\t{AssetName}";
        }
    }
}