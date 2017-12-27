using System;
using System.Collections.Generic;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Extensions;

namespace Neo.Gui.ViewModels.Accounts
{
    public class ImportPrivateKeyViewModel : ViewModelBase, IDialogViewModel<ImportPrivateKeyDialogResult>
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

        public IEnumerable<string> WifStrings
        {
            get
            {
                if (string.IsNullOrEmpty(this.PrivateKeysWif)) return new string[0];

                return this.PrivateKeysWif.ToLines();
            }
        }

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(() => this.Close(this, EventArgs.Empty));
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

        public event EventHandler<ImportPrivateKeyDialogResult> SetDialogResultAndClose;

        public ImportPrivateKeyDialogResult DialogResult { get; private set; }
        #endregion

        #region Private Methods 
        private void Ok()
        {
            if (!this.OkEnabled) return;

            this.walletController.ImportPrivateKeys(this.WifStrings);

            this.Close(this, EventArgs.Empty);
        }
        #endregion
    }
}