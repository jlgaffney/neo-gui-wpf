using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.UI.Core.Wallet;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ViewPrivateKeyViewModel : 
        ViewModelBase,
        IDialogViewModel<ViewPrivateKeyLoadParameters>
    {
        #region Private fields
        private readonly IWalletController walletController;

        private string address;
        private string publicKeyHex;
        private string privateKeyHex;
        private string privateKeyWif;
        #endregion

        #region Constructor
        public ViewPrivateKeyViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;
        }
        #endregion

        #region Public Properties
        public string Address
        {
            get
            {
                return this.address;
            }
            set
            {
                if (this.address == value) return;

                this.address = value;
                this.RaisePropertyChanged();
            }
        }

        public string PublicKeyHex
        {
            get
            {
                return this.publicKeyHex;
            }
            set
            {
                if (this.publicKeyHex == value) return;

                this.publicKeyHex = value;
                this.RaisePropertyChanged();
            }
        }

        public string PrivateKeyHex
        {
            get
            {
                return this.privateKeyHex;
            }
            set
            {
                if (this.privateKeyHex == value) return;

                this.privateKeyHex = value;
                this.RaisePropertyChanged();
            }
        }

        public string PrivateKeyWif
        {
            get
            {
                return this.privateKeyWif;
            }
            set
            {
                if (this.privateKeyWif == value) return;

                this.privateKeyWif = value;
                this.RaisePropertyChanged();
            }
        }

        public RelayCommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion Public Properties

        #region ILoadableDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ViewPrivateKeyLoadParameters parameters)
        {
            if (parameters == null || parameters.ScriptHash == null) return;

            var accountKeys = this.walletController.GetAccountKeys(parameters.ScriptHash);

            this.Address = accountKeys.Address;
            this.PublicKeyHex = accountKeys.PublicKeyHex;
            this.PrivateKeyHex = accountKeys.PrivateKeyHex;
            this.PrivateKeyWif = accountKeys.PrivateKeyWif;
        }
        #endregion
    }
}
