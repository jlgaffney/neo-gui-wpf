using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Wpf.MVVM;
using Neo.SmartContract;
using Neo.VM;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Assets
{
    public class AssetRegistrationViewModel : ViewModelBase
    {
        private static readonly AssetType[] assetTypes = { AssetType.Share, AssetType.Token };

        private readonly IMessagePublisher messagePublisher;

        private AssetType? selectedAssetType;
        private ECPoint selectedOwner;
        private string selectedAdmin;
        private string selectedIssuer;

        private string name;

        private bool totalIsLimited;
        private string totalLimit;

        private int precision = 8;

        private bool formValid;

        public AssetRegistrationViewModel(
            IWalletController walletController,
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;

            this.AssetTypes = new ObservableCollection<AssetType>(assetTypes);

            var contracts = walletController.GetContracts().ToList();

            this.Owners = new ObservableCollection<ECPoint>(contracts.Where(p => p.IsStandard).Select(p => walletController.GetKey(p.PublicKeyHash).PublicKey));
            this.Admins = new ObservableCollection<string>(contracts.Select(p => p.Address));
            this.Issuers = new ObservableCollection<string>(contracts.Select(p => p.Address));
        }

        public ObservableCollection<AssetType> AssetTypes { get; }

        public ObservableCollection<ECPoint> Owners { get; }

        public ObservableCollection<string> Admins { get; }

        public ObservableCollection<string> Issuers { get; }

        public AssetType? SelectedAssetType
        {
            get => this.selectedAssetType;
            set
            {
                if (this.selectedAssetType == value) return;

                this.selectedAssetType = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
                NotifyPropertyChanged(nameof(this.PrecisionEnabled));

                if (!this.PrecisionEnabled)
                {
                    this.Precision = 0;
                }

                CheckForm();
            }
        }

        public ECPoint SelectedOwner
        {
            get => this.selectedOwner;
            set
            {
                if (Equals(this.selectedOwner, value)) return;

                this.selectedOwner = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string SelectedAdmin
        {
            get => this.selectedAdmin;
            set
            {
                if (this.selectedAdmin == value) return;

                this.selectedAdmin = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string SelectedIssuer
        {
            get => this.selectedIssuer;
            set
            {
                if (this.selectedIssuer == value) return;

                this.selectedIssuer = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string Name
        {
            get => this.name;
            set
            {
                if (this.name == value) return;

                this.name = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public bool TotalIsLimited
        {
            get => this.totalIsLimited;
            set
            {
                if (this.totalIsLimited == value) return;

                this.totalIsLimited = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.TotalLimit));
                NotifyPropertyChanged(nameof(this.OkEnabled));

                CheckForm();
            }
        }

        public string TotalLimit
        {
            get => this.totalLimit;
            set
            {
                if (this.totalLimit == value) return;

                this.totalLimit = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.OkEnabled));
            }
        }

        public int Precision
        {
            get => this.precision;
            set
            {
                if (this.precision == value) return;

                this.precision = value;

                NotifyPropertyChanged();
            }
        }

        public bool PrecisionEnabled => this.SelectedAssetType != AssetType.Share;

        public bool OkEnabled => 
            this.SelectedAssetType != null &&
            !string.IsNullOrEmpty(this.Name) &&
            (!this.TotalIsLimited || !string.IsNullOrEmpty(this.TotalLimit)) &&
            this.SelectedOwner != null &&
            !string.IsNullOrWhiteSpace(this.SelectedAdmin) &&
            !string.IsNullOrWhiteSpace(this.SelectedIssuer) &&

            // Check if form is valid
            !this.formValid;

        public ICommand OkCommand => new RelayCommand(this.Ok);

        private InvocationTransaction GenerateTransaction()
        {
            var assetType = this.SelectedAssetType;
            var formattedName = !string.IsNullOrWhiteSpace(this.Name)
                ? $"[{{\"lang\":\"{CultureInfo.CurrentCulture.Name}\",\"name\":\"{this.Name}\"}}]"
                : string.Empty;
            var amount = this.TotalIsLimited ? Fixed8.Parse(this.TotalLimit) : -Fixed8.Satoshi;
            var precisionByte = (byte) this.Precision;
            var owner = this.SelectedOwner;
            var admin = Wallet.ToScriptHash(this.SelectedAdmin);
            var issuer = Wallet.ToScriptHash(this.SelectedIssuer);
            using (var builder = new ScriptBuilder())
            {
                builder.EmitSysCall("Neo.Asset.Create", assetType, formattedName, amount, precisionByte, owner, admin, issuer);
                return new InvocationTransaction
                {
                    Attributes = new[]
                    {
                        new TransactionAttribute
                        {
                            Usage = TransactionAttributeUsage.Script,
                            Data = Contract.CreateSignatureRedeemScript(owner).ToScriptHash().ToArray()
                        }
                    },
                    Script = builder.ToArray()
                };
            }
        }

        private void CheckForm()
        {
            if (!this.OkEnabled) return;

            try
            {
                Wallet.ToScriptHash(this.SelectedAdmin);
                Wallet.ToScriptHash(this.SelectedIssuer);

                this.formValid = true;
            }
            catch (FormatException)
            {
                this.formValid = false;
            }
        }

        private void Ok()
        {
            this.CheckForm();

            if (!this.OkEnabled) return;

            var transaction = this.GenerateTransaction();

            if (transaction == null) return;

            this.messagePublisher.Publish(new InvokeContractMessage(transaction));
            this.TryClose();
        }
    }
}