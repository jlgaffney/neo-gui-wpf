using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Assets;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;

namespace Neo.Gui.ViewModels.Assets
{
    public class AssetDistributionViewModel : ViewModelBase, IDialogViewModel<AssetDistributionLoadParameters>
    {
        #region Private Fields 
        private readonly IWalletController walletController;

        private AssetDto asset;
        private string assetId;
        private bool assetIdEnabled = true;
        private string owner;
        private string admin;
        private string total;
        private string issued;
        private bool distributionEnabled;
        #endregion

        #region Public Properties 
        public ObservableCollection<TransactionOutputItem> Items { get; }

        public AssetDto Asset
        {
            get => this.asset;
            set
            {
                if (this.asset == value) return;

                this.asset = value;
                this.RaisePropertyChanged();
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

        public RelayCommand ConfirmCommand => new RelayCommand(this.Confirm);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Construtor 
        public AssetDistributionViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;

            this.Items = new ObservableCollection<TransactionOutputItem>();
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(AssetDistributionLoadParameters parameters)
        {
            this.AssetId = parameters.AssetStateId;
            this.AssetIdEnabled = false;
        }
        #endregion

        #region Private Methods 
        private void Confirm()
        {
            this.walletController.IssueAsset(this.Asset.Id, this.Items);

            this.Close(this, EventArgs.Empty);
        }

        public void UpdateConfirmButtonEnabled()
        {
            RaisePropertyChanged(nameof(this.ConfirmEnabled));
        }

        private void UpdateAssetDetails()
        {
            var assetState = this.walletController.GetAssetState(this.AssetId);
            this.Asset = new AssetDto { Id = this.AssetId };

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
                this.Owner = assetState.Owner;
                this.Admin = assetState.Admin;
                this.Total = assetState.Total;
                this.Issued = assetState.Issued;
                this.DistributionEnabled = assetState.DistributionEnabled;
            }

            this.Items.Clear();
        }
        #endregion
    }
}