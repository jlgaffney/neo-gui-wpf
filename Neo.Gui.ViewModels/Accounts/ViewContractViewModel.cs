using System;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Wallets;

using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ViewContractViewModel : ViewModelBase, IDialogViewModel<ViewContractDialogResult>, ILoadable
    {
        #region Public Properties 
        public string Address { get; private set; }

        public string ScriptHash { get; private set; }

        public string ParameterList { get; private set; }

        public string RedeemScriptHex { get; private set; }

        public ICommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region IDialogViewModel Implementation 
        public event EventHandler Close;

        public event EventHandler<ViewContractDialogResult> SetDialogResultAndClose;

        public ViewContractDialogResult DialogResult { get; private set; }
        #endregion

        #region ILoadable implementation 
        public void OnLoad(params object[] parameters)
        {
            if (!parameters.Any())
            {
                return;
            }

            var viewContractLoadParameters = parameters[0] as ViewContractLoadParameters;

            this.SetContract(viewContractLoadParameters.Contract);
        }
        #endregion

        #region Private Methods 
        private void SetContract(VerificationContract contract)
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