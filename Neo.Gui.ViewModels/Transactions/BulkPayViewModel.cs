using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Wallets;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Dialogs.Results.Transactions;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;
using Neo.UI.Core.Extensions;

namespace Neo.Gui.ViewModels.Transactions
{
    public class BulkPayViewModel : ViewModelBase,
        IResultDialogViewModel<BulkPayLoadParameters, BulkPayDialogResult>
    {
        private readonly IWalletController walletController;

        private bool assetSelectionEnabled;

        private AssetDescriptor selectedAsset;

        private string addressesAndAmounts;
        
        public BulkPayViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

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

                RaisePropertyChanged();
            }
        }

        public AssetDescriptor SelectedAsset
        {
            get => this.selectedAsset;
            set
            {
                if (this.selectedAsset == value) return;

                this.selectedAsset = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.AssetBalance));
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string AssetBalance => this.GetSelectedAssetBalance();

        public string AddressesAndAmounts
        {
            get => this.addressesAndAmounts;
            set
            {
                if (this.addressesAndAmounts == value) return;

                this.addressesAndAmounts = value;

                RaisePropertyChanged();
            }
        }

        public bool OkEnabled => this.SelectedAsset != null && !string.IsNullOrEmpty(this.AddressesAndAmounts);

        public ICommand OkCommand => new RelayCommand(this.Ok);


        #region ILoadableDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<BulkPayDialogResult> SetDialogResultAndClose;

        public void OnDialogLoad(BulkPayLoadParameters parameters)
        {
            var asset = parameters?.Asset;

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
        }

        #endregion

        private void Ok()
        {
            if (!this.OkEnabled) return;

            var outputs = this.GenerateOutputs();

            var result = new BulkPayDialogResult(outputs);

            this.SetDialogResultAndClose?.Invoke(this, result);
        }

        private string GetSelectedAssetBalance()
        {
            if (this.SelectedAsset == null) return null;

            if (this.SelectedAsset.AssetId is UInt160 scriptHash)
            {
                return this.walletController.GetAvailable(scriptHash).ToString();
            }
            else
            {
                return this.walletController.GetAvailable((UInt256)this.SelectedAsset.AssetId).ToString();
            }
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
                    ScriptHash = this.walletController.AddressToScriptHash(lineElements[0])
                };
            }).Where(p => p.Value.Value != 0).ToArray();
        }
    }
}