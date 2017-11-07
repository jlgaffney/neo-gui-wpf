using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Neo.Core;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Wallets
{
    public class TradeVerificationViewModel : ViewModelBase
    {
        private readonly IDispatcher dispatcher;

        public TradeVerificationViewModel(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

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

        public void SetOutputs(IEnumerable<TransactionOutput> outputs)
        {
            this.dispatcher.InvokeOnMainUIThread(() =>
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
            });
        }
    }
}