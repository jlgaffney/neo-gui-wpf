using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.Properties;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.MVVM;
using Neo.Wallets;

namespace Neo.UI.Transactions
{
    public class PayToViewModel : ViewModelBase
    {
        private readonly IDispatcher dispatcher;

        private bool assetSelectionEnabled;
        private AssetDescriptor selectedAsset;

        private bool payToAddressReadOnly;
        private string payToAddress;

        private string amount;

        private TxOutListBoxItem output;

        public PayToViewModel(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

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
            : ApplicationContext.Instance.CurrentWallet.GetAvailable(this.SelectedAsset.AssetId).ToString();

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
                    Wallet.ToScriptHash(this.PayToAddress);
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
            this.dispatcher.InvokeOnMainUIThread(() =>
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
                    foreach (var assetId in ApplicationContext.Instance.CurrentWallet.FindUnspentCoins()
                        .Select(p => p.Output.AssetId).Distinct())
                    {
                        this.Assets.Add(new AssetDescriptor(assetId));
                    }

                    // Add NEP-5 assets to list
                    foreach (var s in Settings.Default.NEP5Watched)
                    {
                        UInt160 assetId;
                        try
                        {
                            assetId = UInt160.Parse(s);
                        }
                        catch
                        {
                            continue;
                        }

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
                    this.PayToAddress = Wallet.ToAddress(scriptHash);
                    this.PayToAddressReadOnly = true;
                }
            });
        }

        private void Ok()
        {
            this.output = this.GenerateOutput();

            this.TryClose();
        }

        public TxOutListBoxItem GetOutput()
        {
            return this.output;
        }

        private TxOutListBoxItem GenerateOutput()
        {
            var asset = this.SelectedAsset;

            if (asset == null) return null;

            return new TxOutListBoxItem
            {
                AssetName = asset.AssetName,
                AssetId = asset.AssetId,
                Value = new BigDecimal(Fixed8.Parse(this.Amount).GetData(), 8),
                ScriptHash = Wallet.ToScriptHash(this.PayToAddress)
            };
        }
    }
}