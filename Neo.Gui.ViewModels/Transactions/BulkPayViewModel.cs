using System;
using System.Collections.ObjectModel;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

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
        #region Private Fields 
        private readonly IWalletController walletController;
        private bool assetSelectionEnabled;
        private AssetDto selectedAsset;
        private string addressesAndAmounts;
        #endregion

        #region Public Properties 
        public ObservableCollection<AssetDto> Assets { get; }

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

        public AssetDto SelectedAsset
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

        public RelayCommand OkCommand => new RelayCommand(this.Ok);
        #endregion

        #region Constructor 
        public BulkPayViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

            this.Assets = new ObservableCollection<AssetDto>();
        }
        #endregion

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
                    this.Assets.Add(new AssetDto { Id = assetId.ToString(), TokenType = TokenType.FirstClassToken });
                }

                // Add NEP-5 assets to list
                var nep5WatchScriptHashes = this.walletController.GetNEP5WatchScriptHashes();

                foreach (var assetId in nep5WatchScriptHashes)
                {
                    AssetDto nep5Asset;
                    try
                    {
                        nep5Asset = new AssetDto { Id = assetId.ToString(), TokenType = TokenType.NEP5Token };
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

        #region Private Methods 
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

            if (this.SelectedAsset.TokenType == TokenType.NEP5Token)
            {
                return this.walletController.GetNEP5TokenAvailability(this.SelectedAsset.Id);
            }
            else
            {
                return this.walletController.GetFirstClassTokenAvailability(this.SelectedAsset.Id);
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
                    AssetName = this.SelectedAsset.Name,
                    AssetId = UIntBase.Parse(this.SelectedAsset.Id),
                    Value = new BigDecimal(Fixed8.Parse(lineElements[1]).GetData(), 8),
                    ScriptHash = this.walletController.AddressToScriptHash(lineElements[0])
                };
            }).Where(p => p.Value.Value != 0).ToArray();
        }
        #endregion
    }
}