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
using Neo.Gui.Base.Services;

namespace Neo.Gui.ViewModels.Transactions
{
    public class PayToViewModel : ViewModelBase,
        ILoadableDialogViewModel<PayToDialogResult, PayToLoadParameters>
    {
        private readonly IWalletController walletController;
        private readonly IDispatchService dispatchService;

        private bool assetSelectionEnabled;
        private AssetDescriptor selectedAsset;

        private bool payToAddressReadOnly;
        private string payToAddress;

        private string amount;

        public PayToViewModel(
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

        public bool PayToAddressReadOnly
        {
            get => this.payToAddressReadOnly;
            set
            {
                if (this.payToAddressReadOnly == value) return;

                this.payToAddressReadOnly = value;

                RaisePropertyChanged();
            }
        }

        public string PayToAddress
        {
            get => this.payToAddress;
            set
            {
                if (this.payToAddress == value) return;

                this.payToAddress = value;
                
                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string Amount
        {
            get => this.amount;
            set
            {
                if (this.amount == value) return;

                this.amount = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public bool OkEnabled
        {
            get
            {
                if (this.SelectedAsset == null ||
                    string.IsNullOrEmpty(this.PayToAddress) ||
                    string.IsNullOrEmpty(this.Amount)) return false;
                
                try
                {
                    this.walletController.ToScriptHash(this.PayToAddress);
                }
                catch (FormatException)
                {
                    return false;
                }

                if (!Fixed8.TryParse(this.Amount, out var parsedAmount)) return false;

                var asset = this.SelectedAsset;

                if (asset == null) return false;

                if (parsedAmount.GetData() % (long) Math.Pow(10, 8 - asset.Decimals) != 0) return false;

                if (parsedAmount == Fixed8.Zero) return false;

                return true;
            }
        }

        public ICommand OkCommand => new RelayCommand(this.Ok);


        #region ILoadableDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<PayToDialogResult> SetDialogResultAndClose;

        public PayToDialogResult DialogResult { get; private set; }
        
        public void OnDialogLoad(PayToLoadParameters parameters)
        {
            var asset = parameters?.Asset;
            var scriptHash = parameters?.ScriptHash;

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
                    foreach (var assetId in this.walletController.GetNEP5WatchScriptHashes())
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

                if (scriptHash != null)
                {
                    this.PayToAddress = this.walletController.ToAddress(scriptHash);
                    this.PayToAddressReadOnly = true;
                }
            });
        }
        #endregion

        private void Ok()
        {
            if (!this.OkEnabled) return;

            var output = this.GenerateOutput();

            var result = new PayToDialogResult(output);

            this.SetDialogResultAndClose?.Invoke(this, result);
        }

        private TransactionOutputItem GenerateOutput()
        {
            var asset = this.SelectedAsset;

            if (asset == null) return null;

            return new TransactionOutputItem
            {
                AssetName = asset.AssetName,
                AssetId = asset.AssetId,
                Value = new BigDecimal(Fixed8.Parse(this.Amount).GetData(), 8),
                ScriptHash = this.walletController.ToScriptHash(this.PayToAddress)
            };
        }
    }
}