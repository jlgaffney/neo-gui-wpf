using System.Collections.Generic;
using Neo.Core;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Wallets
{
    public class TradeVerificationViewModel : ViewModelBase
    {

        public void SetOutputs(IEnumerable<TransactionOutput> outputs)
        {
            foreach (var output in outputs)
            {
                var asset = Blockchain.Default.GetAssetState(output.AssetId);

                /*this.ListBox.Items.Add(new TxOutListBoxItem
                {
                    AssetName = $"{asset.GetName()} ({asset.Owner})",
                    AssetId = output.AssetId,
                    Value = new BigDecimal(output.Value.GetData(), 8),
                    ScriptHash = output.ScriptHash
                });*/
            }
        }
    }
}
