using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ViewContractViewModel : 
        ViewModelBase, 
        IDialogViewModel<ViewContractLoadParameters>
    {
        #region Private fields
        private readonly IWalletController walletController;

        private string address;
        private string scriptHash;
        private string parameterList;
        private string redeemScriptHex;
        #endregion

        #region Constructor
        public ViewContractViewModel(
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

        public string ScriptHash
        {
            get
            {
                return this.scriptHash;
            }
            set
            {
                if (this.scriptHash == value) return;

                this.scriptHash = value;
                this.RaisePropertyChanged();
            }
        }

        public string ParameterList
        {
            get
            {
                return this.parameterList;
            }
            set
            {
                if (this.parameterList == value) return;

                this.parameterList = value;
                this.RaisePropertyChanged();
            }
        }

        public string RedeemScriptHex
        {
            get
            {
                return this.redeemScriptHex;
            }
            set
            {
                if (this.redeemScriptHex == value) return;

                this.redeemScriptHex = value;
                this.RaisePropertyChanged();
            }
        }

        public RelayCommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region ILoadableDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ViewContractLoadParameters parameters)
        {
            if (parameters?.ScriptHash == null) return;

            var accountContract = this.walletController.GetAccountContract(parameters.ScriptHash);

            this.Address = accountContract.Address;
            this.ScriptHash = accountContract.ScriptHash;
            this.ParameterList = accountContract.ParameterList;
            this.RedeemScriptHex = accountContract.RedeemScriptHex;
        }
        #endregion
    }
}