using System;
using Neo.Core;
using Neo.Wallets;

namespace Neo.UI
{
    internal class TxOutListBoxItem
    {
        public string AssetName;
        public UIntBase AssetId;
        public BigDecimal Value;
        public UInt160 ScriptHash;

        public override string ToString()
        {
            return $"{Wallet.ToAddress(ScriptHash)}\t{Value}\t{AssetName}";
        }

        public TransactionOutput ToTxOutput()
        {
            if (AssetId is UInt256 assetId && Value.Decimals == 8)
            {
                return new TransactionOutput
                {
                    AssetId = assetId,
                    Value = new Fixed8((long) Value.Value),
                    ScriptHash = ScriptHash
                };
            }

            throw new NotSupportedException();
        }
    }
}
