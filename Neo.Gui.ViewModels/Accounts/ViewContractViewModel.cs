using System;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.SmartContract;

using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Base.Dialogs.Results.Wallets;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ViewContractViewModel : ViewModelBase, 
        ILoadableDialogViewModel<ViewContractDialogResult, ViewContractLoadParameters>
    {
        #region Public Properties 
        public string Address { get; private set; }

        public string ScriptHash { get; private set; }

        public string ParameterList { get; private set; }

        public string RedeemScriptHex { get; private set; }

        public ICommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region ILoadableDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<ViewContractDialogResult> SetDialogResultAndClose;

        public ViewContractDialogResult DialogResult { get; private set; }
        
        public void OnDialogLoad(ViewContractLoadParameters parameters)
        {
            if (parameters?.Contract == null) return;

            this.SetContract(parameters.Contract);
        }
        #endregion

        #region Private Methods 
        private void SetContract(Contract contract)
        {
            this.Address = contract.Address;
            this.ScriptHash = contract.ScriptHash.ToString();
            this.ParameterList = contract.ParameterList.Cast<byte>().ToArray().ToHexString();

            this.RedeemScriptHex = contract.Script.ToHexString();

            // Update properties
            RaisePropertyChanged(nameof(this.Address));
            RaisePropertyChanged(nameof(this.ScriptHash));
            RaisePropertyChanged(nameof(this.ParameterList));
            RaisePropertyChanged(nameof(this.RedeemScriptHex));
        }
        #endregion
    }
}