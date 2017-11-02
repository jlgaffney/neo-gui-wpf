using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.UI.Base.Extensions;
using Neo.UI.Base.MVVM;
using Neo.Wallets;

namespace Neo.UI.Transactions
{
    public class BulkPayViewModel : ViewModelBase
    {
        private bool assetSelectionEnabled;

        private AssetDescriptor selectedAsset;

        private string addressesAndAmounts;

        private TxOutListBoxItem[] outputs;

        public BulkPayViewModel()
        {
            this.Assets = new ObservableCollection<AssetDescriptor>();
        }

        public ObservableCollection<AssetDescriptor> Assets { get; }

        public bool AssetSelectionEnabled
        {
            get => this.assetSelectionEnabled;
            set
            {
                if (this.assetSelectionEnabled == value) return;

                this.assetSelectionEnabled = value;

                NotifyPropertyChanged();
            }
        }

        public AssetDescriptor SelectedAsset
        {
            get => this.selectedAsset;
            set
            {
                if (this.selectedAsset == value) return;

                this.selectedAsset = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.AssetBalance));
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string AssetBalance => this.SelectedAsset?.GetAvailable().ToString();

        public string AddressesAndAmounts
        {
            get => this.addressesAndAmounts;
            set
            {
                if (this.addressesAndAmounts == value) return;

                this.addressesAndAmounts = value;

                NotifyPropertyChanged();
            }
        }

        public bool OkEnabled => this.SelectedAsset != null && !string.IsNullOrEmpty(this.AddressesAndAmounts);

        public ICommand OkCommand => new RelayCommand(this.Ok);


        internal void Load(AssetDescriptor asset = null)
        {
            this.Assets.Clear();

            if (asset != null)
            {
                this.Assets.Add(asset);
                this.SelectedAsset = asset;
                this.AssetSelectionEnabled = false;
            }
            else
            {
                this.Assets.AddRange(AssetDescriptor.GetAssets());

                this.AssetSelectionEnabled = this.Assets.Any();
            }
        }

        private void Ok()
        {
            this.outputs = this.GenerateOutputs();

            this.TryClose();
        }

        internal TxOutListBoxItem[] GetOutputs()
        {
            return this.outputs;
        }

        private TxOutListBoxItem[] GenerateOutputs()
        {
            if (this.SelectedAsset == null || string.IsNullOrEmpty(this.AddressesAndAmounts)) return null;

            var lines = this.AddressesAndAmounts.ToLines();

            if (lines == null || !lines.Any()) return null;

            return lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line =>
            {
                var lineElements = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

                return new TxOutListBoxItem
                {
                    AssetName = this.SelectedAsset.AssetName,
                    AssetId = this.SelectedAsset.AssetId,
                    Value = new BigDecimal(Fixed8.Parse(lineElements[1]).GetData(), 8),
                    ScriptHash = Wallet.ToScriptHash(lineElements[0])
                };
            }).Where(p => p.Value.Value != 0).ToArray();
        }
    }
}