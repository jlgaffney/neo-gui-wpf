using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Wpf.MVVM;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Assets
{
    public class AssetDistributionViewModel : ViewModelBase
    {
        private readonly IWalletController walletController;
        private readonly IMessagePublisher messagePublisher;
        private readonly IDispatchHelper dispatchHelper;

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
            IDispatchHelper dispatchHelper)
        {
            this.walletController = walletController;
            this.messagePublisher = messagePublisher;
            this.dispatchHelper = dispatchHelper;

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

                NotifyPropertyChanged();
            }
        }

        public string AssetId
        {
            get => this.assetId;
            set
            {
                if (this.assetId == value) return;

                this.assetId = value;

                NotifyPropertyChanged();

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

                NotifyPropertyChanged();
            }
        }

        public string Owner
        {
            get => this.owner;
            set
            {
                if (this.owner == value) return;

                this.owner = value;

                NotifyPropertyChanged();
            }
        }

        public string Admin
        {
            get => this.admin;
            set
            {
                if (this.admin == value) return;

                this.admin = value;

                NotifyPropertyChanged();
            }
        }

        public string Total
        {
            get => this.total;
            private set
            {
                if (this.total == value) return;

                this.total = value;

                NotifyPropertyChanged();
            }
        }

        public string Issued
        {
            get => this.issued;
            set
            {
                if (this.issued == value) return;

                this.issued = value;

                NotifyPropertyChanged();
            }
        }

        public bool DistributionEnabled
        {
            get => this.distributionEnabled;
            set
            {
                if (this.distributionEnabled == value) return;

                this.distributionEnabled = value;

                NotifyPropertyChanged();
            }
        }

        public bool ConfirmEnabled => this.Items.Count > 0;

        public ICommand ConfirmCommand => new RelayCommand(this.Confirm);

        public ICommand CancelCommand => new RelayCommand(this.TryClose);

        
        private void Confirm()
        {
            var transaction = this.GenerateTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new SignTransactionAndShowInformationMessage(transaction));
            this.TryClose();
        }

        public void UpdateConfirmButtonEnabled()
        {
            NotifyPropertyChanged(nameof(this.ConfirmEnabled));
        }

        internal void SetAsset(AssetState assetState)
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
                this.Admin = Wallet.ToAddress(assetState.Admin);
                this.Total = assetState.Amount == -Fixed8.Satoshi ? "+\u221e" : assetState.Amount.ToString();
                this.Issued = assetState.Available.ToString();
                this.DistributionEnabled = true;
            }

            this.dispatchHelper.InvokeOnMainUIThread(() => this.Items.Clear());
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