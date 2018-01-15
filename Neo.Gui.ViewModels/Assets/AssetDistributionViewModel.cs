using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.Wallets;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Assets;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;

namespace Neo.Gui.ViewModels.Assets
{
    public class AssetDistributionViewModel : ViewModelBase, IDialogViewModel<AssetDistributionLoadParameters>
    {
        private readonly IWalletController walletController;

        private AssetDescriptor asset;

        private string assetId;

        private bool assetIdEnabled = true;

        private string owner;
        private string admin;
        private string total;
        private string issued;

        private bool distributionEnabled;

        public AssetDistributionViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

            this.Items = new ObservableCollection<TransactionOutputItem>();
        }

        public ObservableCollection<TransactionOutputItem> Items { get; }

        public AssetDescriptor Asset
        {
            get => this.asset;
            set
            {
                if (this.asset == value) return;

                this.asset = value;

                RaisePropertyChanged();
            }
        }

        public string AssetId
        {
            get => this.assetId;
            set
            {
                if (this.assetId == value) return;

                this.assetId = value;

                RaisePropertyChanged();

                // Update asset details
                this.UpdateAssetDetails();
            }
        }

        public bool AssetIdEnabled
        {
            get => this.assetIdEnabled;
            set
            {
                if (this.assetIdEnabled == value) return;

                this.assetIdEnabled = value;

                RaisePropertyChanged();
            }
        }

        public string Owner
        {
            get => this.owner;
            set
            {
                if (this.owner == value) return;

                this.owner = value;

                RaisePropertyChanged();
            }
        }

        public string Admin
        {
            get => this.admin;
            set
            {
                if (this.admin == value) return;

                this.admin = value;

                RaisePropertyChanged();
            }
        }

        public string Total
        {
            get => this.total;
            private set
            {
                if (this.total == value) return;

                this.total = value;

                RaisePropertyChanged();
            }
        }

        public string Issued
        {
            get => this.issued;
            set
            {
                if (this.issued == value) return;

                this.issued = value;

                RaisePropertyChanged();
            }
        }

        public bool DistributionEnabled
        {
            get => this.distributionEnabled;
            set
            {
                if (this.distributionEnabled == value) return;

                this.distributionEnabled = value;

                RaisePropertyChanged();
            }
        }

        public bool ConfirmEnabled => this.Items.Count > 0;

        public ICommand ConfirmCommand => new RelayCommand(this.Confirm);

        public ICommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(AssetDistributionLoadParameters parameters)
        {
        }
        #endregion

        private void Confirm()
        {
            this.walletController.IssueAsset((UInt256)this.Asset.AssetId, this.Items);

            this.Close(this, EventArgs.Empty);
        }

        public void UpdateConfirmButtonEnabled()
        {
            RaisePropertyChanged(nameof(this.ConfirmEnabled));
        }

        public void SetAsset(AssetState assetState)
        {
            if (assetState == null) return;

            this.AssetId = assetState.AssetId.ToString();
            this.AssetIdEnabled = false;
        }

        private void UpdateAssetDetails()
        {
            AssetState assetState;
            if (UInt256.TryParse(this.AssetId, out var id))
            {
                assetState = this.walletController.GetAssetState(id);
                this.Asset = new AssetDescriptor(id);
            }
            else
            {
                assetState = null;
                this.Asset = null;
            }

            if (assetState == null)
            {
                this.Owner = string.Empty;
                this.Admin = string.Empty;
                this.Total = string.Empty;
                this.Issued = string.Empty;
                this.DistributionEnabled = false;
            }
            else
            {
                this.Owner = assetState.Owner.ToString();
                this.Admin = this.walletController.ScriptHashToAddress(assetState.Admin);
                this.Total = assetState.Amount == -Fixed8.Satoshi ? "+\u221e" : assetState.Amount.ToString();
                this.Issued = assetState.Available.ToString();
                this.DistributionEnabled = true;
            }

            this.Items.Clear();
        }
    }
}