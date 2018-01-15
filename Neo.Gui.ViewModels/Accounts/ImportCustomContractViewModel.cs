using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.UI.Core.Controllers.Interfaces;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ImportCustomContractViewModel : ViewModelBase, IDialogViewModel<ImportCustomContractLoadParameters>
    {
        #region Private Fields 
        private readonly IWalletController walletController;
        
        private string parameterList;
        private string script;
        #endregion

        #region Public Properties 
        public string ParameterList
        {
            get => this.parameterList;
            set
            {
                if (this.parameterList == value) return;

                this.parameterList = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public string Script
        {
            get => this.script;
            set
            {
                if (this.script == value) return;

                this.script = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public bool ConfirmEnabled =>
            !string.IsNullOrEmpty(this.ParameterList) &&
            !string.IsNullOrEmpty(this.Script);

        public RelayCommand ConfirmCommand => new RelayCommand(this.Confirm);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public ImportCustomContractViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ImportCustomContractLoadParameters parameters)
        {
        }
        #endregion

        #region Private Methods 
        private void Confirm()
        {
            if (!this.ConfirmEnabled) return;

            this.walletController.AddContractWithParameters(this.Script, this.ParameterList);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}