using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Neo.Wallets;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Services;

using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public class PayToViewModel : ViewModelBase
    {
        private readonly IWalletController walletController;
        private readonly IDispatchService dispatchService;

        private bool assetSelectionEnabled;
        private AssetDescriptor selectedAsset;

        private bool payToAddressReadOnly;
        private string payToAddress;

        private string amount;

        private TransactionOutputItem output;

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

        public bool PayToAddressReadOnly
        {
            get => this.payToAddressReadOnly;
            set
            {
                if (this.payToAddressReadOnly == value) return;

                this.payToAddressReadOnly = value;

                NotifyPropertyChanged();
            }
        }

        public string PayToAddress
        {
            get => this.payToAddress;
            set
            {
                if (this.payToAddress == value) return;

                this.payToAddress = value;
                
                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string Amount
        {
            get => this.amount;
            set
            {
                if (this.amount == value) return;

                this.amount = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
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


        public void Load(AssetDescriptor asset = null, UInt160 scriptHash = null)
        {
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

        private void Ok()
        {
            this.output = this.GenerateOutput();

            //this.TryClose();
        }

        public TransactionOutputItem GetOutput()
        {
            return this.output;
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