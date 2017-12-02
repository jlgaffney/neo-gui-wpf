using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Wpf.MVVM;
using Neo.Wallets;
using NeoSettings = Neo.Gui.Wpf.Properties.Settings;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public class BulkPayViewModel : ViewModelBase
    {
        private readonly IWalletController walletController;
        private readonly IDispatchHelper dispatchHelper;

        private bool assetSelectionEnabled;

        private AssetDescriptor selectedAsset;

        private string addressesAndAmounts;

        private TransactionOutputItem[] outputs;

        public BulkPayViewModel(
            IWalletController walletController,
            IDispatchHelper dispatchHelper)
        {
            this.walletController = walletController;
            this.dispatchHelper = dispatchHelper;

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

        public string AssetBalance => this.SelectedAsset == null ? string.Empty
            : this.walletController.GetAvailable(this.SelectedAsset.AssetId).ToString();

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
            this.dispatchHelper.InvokeOnMainUIThread(() =>
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
                    // Add first-class assets to list
                    foreach (var assetId in this.walletController.FindUnspentCoins()
                        .Select(p => p.Output.AssetId).Distinct())
                    {
                        this.Assets.Add(new AssetDescriptor(assetId));
                    }

                    // Add NEP-5 assets to list
                    var nep5WatchScriptHashes = this.walletController.GetNEP5WatchScriptHashes();

                    foreach (var assetId in nep5WatchScriptHashes)
                    {
                        AssetDescriptor nep5Asset;
                        try
                        {
                            nep5Asset = new AssetDescriptor(assetId);
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }

                        this.Assets.Add(nep5Asset);
                    }

                    this.AssetSelectionEnabled = this.Assets.Any();
                }
            });
        }

        private void Ok()
        {
            this.outputs = this.GenerateOutputs();

            this.TryClose();
        }

        internal TransactionOutputItem[] GetOutputs()
        {
            return this.outputs;
        }

        private TransactionOutputItem[] GenerateOutputs()
        {
            if (this.SelectedAsset == null || string.IsNullOrEmpty(this.AddressesAndAmounts)) return null;

            var lines = this.AddressesAndAmounts.ToLines();

            if (lines == null || !lines.Any()) return null;

            return lines.Where(line => !string.IsNullOrWhiteSpace(line)).Select(line =>
            {
                var lineElements = line.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

                return new TransactionOutputItem
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