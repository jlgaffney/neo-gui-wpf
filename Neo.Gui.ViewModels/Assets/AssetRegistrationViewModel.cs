using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Assets;
using Neo.Gui.Dialogs.LoadParameters.Contracts;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.Gui.ViewModels.Assets
{
    public class AssetRegistrationViewModel : ViewModelBase, IDialogViewModel<AssetRegistrationLoadParameters>
    {
        private static readonly AssetType[] assetTypes = { AssetType.Share, AssetType.Token };

        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;

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
            IDialogManager dialogManager,
            IWalletController walletController)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;
            
            this.AssetTypes = new ObservableCollection<AssetType>(assetTypes);

            var nonWatchOnlyAccounts = this.walletController.GetNonWatchOnlyAccounts().ToList();
            var standardAccounts = nonWatchOnlyAccounts.Where(account => account.Contract.IsStandard);

            this.Owners = new ObservableCollection<ECPoint>(standardAccounts.Select(p => p.GetKey().PublicKey));
            this.Admins = new ObservableCollection<string>(nonWatchOnlyAccounts.Select(p => p.Address));
            this.Issuers = new ObservableCollection<string>(nonWatchOnlyAccounts.Select(p => p.Address));
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

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
                RaisePropertyChanged(nameof(this.PrecisionEnabled));

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

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string SelectedAdmin
        {
            get => this.selectedAdmin;
            set
            {
                if (this.selectedAdmin == value) return;

                this.selectedAdmin = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string SelectedIssuer
        {
            get => this.selectedIssuer;
            set
            {
                if (this.selectedIssuer == value) return;

                this.selectedIssuer = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public string Name
        {
            get => this.name;
            set
            {
                if (this.name == value) return;

                this.name = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public bool TotalIsLimited
        {
            get => this.totalIsLimited;
            set
            {
                if (this.totalIsLimited == value) return;

                this.totalIsLimited = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.TotalLimit));
                RaisePropertyChanged(nameof(this.OkEnabled));

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

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public int Precision
        {
            get => this.precision;
            set
            {
                if (this.precision == value) return;

                this.precision = value;

                RaisePropertyChanged();
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

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(AssetRegistrationLoadParameters parameters)
        {
        }
        #endregion

        #region Private methods

        private void CheckForm()
        {
            if (!this.OkEnabled) return;

            var adminAddressIsValid = this.walletController.AddressIsValid(this.SelectedAdmin);
            var issuerAddressIsValid = this.walletController.AddressIsValid(this.SelectedIssuer);

            this.formValid = adminAddressIsValid && issuerAddressIsValid;
        }

        private void Ok()
        {
            this.CheckForm();

            if (!this.OkEnabled) return;

            var transaction = this.MakeTransaction();

            if (transaction == null) return;

            this.dialogManager.ShowDialog(new InvokeContractLoadParameters(transaction));

            this.Close(this, EventArgs.Empty);
        }

        private InvocationTransaction MakeTransaction()
        {
            var assetType = this.SelectedAssetType;
            var formattedName = !string.IsNullOrWhiteSpace(this.Name)
                ? $"[{{\"lang\":\"{CultureInfo.CurrentCulture.Name}\",\"name\":\"{this.Name}\"}}]"
                : string.Empty;
            var amount = this.TotalIsLimited ? Fixed8.Parse(this.TotalLimit) : -Fixed8.Satoshi;
            var precisionByte = (byte)this.Precision;
            var owner = this.SelectedOwner;
            var admin = this.walletController.AddressToScriptHash(this.SelectedAdmin);
            var issuer = this.walletController.AddressToScriptHash(this.SelectedIssuer);

            return this.walletController.MakeAssetCreationTransaction(assetType, formattedName, amount, precisionByte, owner, admin, issuer);
        }

        #endregion
    }
}