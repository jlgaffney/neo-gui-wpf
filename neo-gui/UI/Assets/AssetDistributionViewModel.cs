using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.UI.Base.MVVM;
using Neo.Wallets;

namespace Neo.UI.Assets
{
    internal class AssetDistributionViewModel : ViewModelBase
    {
        private AssetDescriptor asset;

        private string assetId;

        private bool assetIdEnabled = true;

        private string owner;
        private string admin;
        private string cap;
        private string issued;

        private bool distributionEnabled;

        private IssueTransaction transaction;

        public AssetDistributionViewModel()
        {
            this.Items = new ObservableCollection<TxOutListBoxItem>();
        }

        public ObservableCollection<TxOutListBoxItem> Items { get; }

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

        public string Cap
        {
            get => this.cap;
            set
            {
                if (this.cap == value) return;

                this.cap = value;

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
            this.transaction = this.GenerateTransaction();

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
            AssetState state;
            if (UInt256.TryParse(this.AssetId, out var id))
            {
                state = Blockchain.Default.GetAssetState(id);
                this.Asset = new AssetDescriptor(state);
            }
            else
            {
                state = null;
                this.Asset = null;
            }

            if (state == null)
            {
                this.Owner = string.Empty;
                this.Admin = string.Empty;
                this.Cap = string.Empty;
                this.Issued = string.Empty;
                this.DistributionEnabled = false;
            }
            else
            {
                this.Owner = state.Owner.ToString();
                this.Admin = Wallet.ToAddress(state.Admin);
                this.Cap = state.Amount == -Fixed8.Satoshi ? "+\u221e" : state.Amount.ToString();
                this.Issued = state.Available.ToString();
                this.DistributionEnabled = true;
            }

            this.Items.Clear();
        }

        internal IssueTransaction GetTransaction()
        {
            return this.transaction;
        }

        private IssueTransaction GenerateTransaction()
        {
            if (this.Asset == null) return null;
            return App.CurrentWallet.MakeTransaction(new IssueTransaction
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