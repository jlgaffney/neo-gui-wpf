using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Neo.Core;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Wallets
{
    internal class TradeVerificationViewModel : ViewModelBase
    {
        public TradeVerificationViewModel()
        {
            this.Items = new ObservableCollection<TxOutListBoxItem>();
        }

        public ObservableCollection<TxOutListBoxItem> Items { get; }

        public ICommand AcceptCommand => new RelayCommand(this.Accept);

        public ICommand RefuseCommand => new RelayCommand(this.TryClose);


        public bool TradeAccepted { get; set; }

        private void Accept()
        {
            this.TradeAccepted = true;

            this.TryClose();
        }

        internal void SetOutputs(IEnumerable<TransactionOutput> outputs)
        {
            foreach (var output in outputs)
            {
                var asset = Blockchain.Default.GetAssetState(output.AssetId);

                this.Items.Add(new TxOutListBoxItem
                {
                    AssetName = $"{asset.GetName()} ({asset.Owner})",
                    AssetId = output.AssetId,
                    Value = new BigDecimal(output.Value.GetData(), 8),
                    ScriptHash = output.ScriptHash
                });
            }
        }
    }
}