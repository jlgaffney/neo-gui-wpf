using System;
using System.Collections.ObjectModel;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Dialogs.Results.Transactions;
using Neo.UI.Core.Data;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Helpers.Extensions;
using Neo.UI.Core.Wallet;

namespace Neo.Gui.ViewModels.Transactions
{
    public class PayToViewModel : ViewModelBase,
        IResultDialogViewModel<PayToLoadParameters, PayToDialogResult>
    {
        #region Private Fields 
        private readonly IWalletController walletController;

        private bool assetSelectionEnabled;
        private AssetDto selectedAsset;

        private bool payToAddressReadOnly;
        private string payToAddress;

        private string amount;
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
                    this.walletController.AddressToScriptHash(this.PayToAddress);
                }
                catch (FormatException)
                {
                    return false;
                }

                if (!Fixed8.TryParse(this.Amount, out var parsedAmount)) return false;

                var asset = this.SelectedAsset;

                if (asset == null) return false;

                if (parsedAmount.GetData() % (long)Math.Pow(10, 8 - asset.Decimals) != 0) return false;

                if (parsedAmount == Fixed8.Zero) return false;

                return true;
            }
        }

        public RelayCommand OkCommand => new RelayCommand(this.Ok);
        #endregion

        #region Constructor 
        public PayToViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

            this.Assets = new ObservableCollection<AssetDto>();
        }
        #endregion

        #region ILoadableDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<PayToDialogResult> SetDialogResultAndClose;

        public async void OnDialogLoad(PayToLoadParameters parameters)
        {
            var asset = parameters?.Asset;
            var scriptHash = parameters?.ScriptHash;

            this.Assets.Clear();

            if (asset != null)
            {
                this.Assets.Add(asset);
                this.SelectedAsset = asset;
                this.AssetSelectionEnabled = false;
            }
            else
            {
                var walletAssets = await this.walletController.GetWalletAssets();
                this.Assets.AddRange(walletAssets);
                
                this.AssetSelectionEnabled = this.Assets.Any();
            }

            if (scriptHash != null)
            {
                this.PayToAddress = this.walletController.ScriptHashToAddress(scriptHash.ToString());
                this.PayToAddressReadOnly = true;
            }
        }
        #endregion

        #region Private Methods 
        private void Ok()
        {
            if (!this.OkEnabled) return;

            var output = this.GenerateOutput();

            var result = new PayToDialogResult(output);

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

        private TransactionOutputItem GenerateOutput()
        {
            var asset = this.SelectedAsset;

            if (asset == null) return null;

            return new TransactionOutputItem
            {
                AssetName = asset.Name,
                AssetId = UIntBase.Parse(asset.Id),
                Value = new BigDecimal(Fixed8.Parse(this.Amount).GetData(), 8),
                ScriptHash = this.walletController.AddressToScriptHash(this.PayToAddress)
            };
        }
        #endregion
    }
}