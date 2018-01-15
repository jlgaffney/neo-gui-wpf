using System;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Extensions;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ImportPrivateKeyViewModel : ViewModelBase, IDialogViewModel<ImportPrivateKeyLoadParameters>
    {
        #region Private Fields 
        private readonly IWalletController walletController;

        private string privateKeysWif;
        #endregion

        #region Public Properties 
        public bool OkEnabled => !string.IsNullOrEmpty(this.PrivateKeysWif);

        public string PrivateKeysWif
        {
            get => this.privateKeysWif;
            set
            {
                if (this.privateKeysWif == value) return;

                this.privateKeysWif = value;
                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.OkEnabled));
            }
        }

        public RelayCommand OkCommand => new RelayCommand(this.Ok);

        public RelayCommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
        #endregion

        #region Constructor 
        public ImportPrivateKeyViewModel(
            IWalletController walletController)
        {
            this.walletController = walletController;
        }
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(ImportPrivateKeyLoadParameters parameters)
        {
        }
        #endregion

        #region Private Methods 
        private void Ok()
        {
            if (!this.OkEnabled) return;

            if (string.IsNullOrEmpty(this.PrivateKeysWif)) return;

            var wifStrings = this.PrivateKeysWif.ToLines().Where(line => !string.IsNullOrEmpty(line));
            this.walletController.ImportPrivateKeys(wifStrings);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}