using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Wallets;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Base.Dialogs.Results.Transactions;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Services;

namespace Neo.Gui.ViewModels.Transactions
{
    public class BulkPayViewModel : ViewModelBase,
        ILoadableDialogViewModel<BulkPayDialogResult, BulkPayLoadParameters>
    {
        private readonly IWalletController walletController;
        private readonly IDispatchService dispatchService;

        private bool assetSelectionEnabled;

        private AssetDescriptor selectedAsset;

        private string addressesAndAmounts;
        
        public BulkPayViewModel(
            IWalletController walletController,
            IDispatchService dispatchService)
        {
            this.walletController = walletController;
            this.dispatchService = dispatchService;

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

        public string AssetBalance => this.SelectedAsset == null ? string.Empty
            : this.walletController.GetAvailable(this.SelectedAsset.AssetId).ToString();

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

        public BulkPayDialogResult DialogResult { get; private set; }
        
        public void OnDialogLoad(BulkPayLoadParameters parameters)
        {
            var asset = parameters?.Asset;

            this.dispatchService.InvokeOnMainUIThread(() =>
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
        #endregion

        private void Ok()
        {
            if (!this.OkEnabled) return;

            var outputs = this.GenerateOutputs();

            var result = new BulkPayDialogResult(outputs);

            this.SetDialogResultAndClose?.Invoke(this, result);
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
                    ScriptHash = this.walletController.ToScriptHash(lineElements[0])
                };
            }).Where(p => p.Value.Value != 0).ToArray();
        }
    }
}