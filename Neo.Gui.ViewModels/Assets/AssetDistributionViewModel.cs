using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.Wallets;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Services;

namespace Neo.Gui.ViewModels.Assets
{
    public class AssetDistributionViewModel : ViewModelBase, IDialogViewModel<AssetDistributionDialogResult>
    {
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;
        private readonly IDispatchService dispatchService;

        private AssetDescriptor asset;

        private string assetId;

        private bool assetIdEnabled = true;

        private string owner;
        private string admin;
        private string total;
        private string issued;

        private bool distributionEnabled;

        public AssetDistributionViewModel(
            IWalletController walletController,
            IMessagePublisher messagePublisher,
            IDispatchService dispatchService)
        {
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;
            this.dispatchService = dispatchService;

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

        public event EventHandler<AssetDistributionDialogResult> SetDialogResultAndClose;

        public AssetDistributionDialogResult DialogResult { get; private set; }
        #endregion

        private void Confirm()
        {
            var transaction = this.GenerateTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new SignTransactionAndShowInformationMessage(transaction));
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
                this.Admin = this.walletController.ToAddress(assetState.Admin);
                this.Total = assetState.Amount == -Fixed8.Satoshi ? "+\u221e" : assetState.Amount.ToString();
                this.Issued = assetState.Available.ToString();
                this.DistributionEnabled = true;
            }

            this.dispatchService.InvokeOnMainUIThread(() => this.Items.Clear());
        }

        private IssueTransaction GenerateTransaction()
        {
            if (this.Asset == null) return null;
            return this.walletController.MakeTransaction(new IssueTransaction
            {
                Version = 1,
                Outputs = this.Items.GroupBy(p => p.ScriptHash).Select(g => new TransactionOutput
                {
                    AssetId = (UInt256) this.Asset.AssetId,
                    Value = g.Sum(p => new Fixed8((long)p.Value.Value)),
                    ScriptHash = g.Key
                }).ToArray()
            }, fee: Fixed8.One);
        }
    }
}