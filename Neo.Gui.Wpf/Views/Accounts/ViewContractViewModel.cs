using System;
using System.Linq;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Wpf.MVVM;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Accounts
{
    public class ViewContractViewModel : ViewModelBase, IDialogViewModel<ViewContractDialogResult>, ILoadable
    {
        #region Public Properties 
        public string Address { get; private set; }

        public string ScriptHash { get; private set; }

        public string ParameterList { get; private set; }

        public string RedeemScriptHex { get; private set; }

        public RelayCommand CloseCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
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
            NotifyPropertyChanged(nameof(this.Address));
            NotifyPropertyChanged(nameof(this.ScriptHash));
            NotifyPropertyChanged(nameof(this.ParameterList));
            NotifyPropertyChanged(nameof(this.RedeemScriptHex));
        }
        #endregion
    }
}