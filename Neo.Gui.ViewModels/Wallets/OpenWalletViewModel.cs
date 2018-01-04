using System;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Base.Services.Interfaces;

namespace Neo.Gui.ViewModels.Wallets
{
    public class OpenWalletViewModel : ViewModelBase, IDialogViewModel<OpenWalletDialogResult>
    {
        #region Private Fields
        private readonly IFileDialogService fileDialogService;

        private string walletPath;
        private string password;
        #endregion

        #region Constructor

        public OpenWalletViewModel(
            IFileManager fileManager,
            IFileDialogService fileDialogService,
            ISettingsManager settingsManager)
        {
            this.fileDialogService = fileDialogService;

            if (fileManager.FileExists(settingsManager.LastWalletPath))
            {
                this.WalletPath = settingsManager.LastWalletPath;
            }
        }
        #endregion

        #region Public Properties 
        public string WalletPath
        {
            get => this.walletPath;
            set
            {
                if (this.walletPath == value) return;

                this.walletPath = value;

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public bool ConfirmEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(this.WalletPath) || string.IsNullOrEmpty(this.password))
                {
                    return false;
                }

                return true;
            }
        }

        public ICommand GetWalletPathCommand => new RelayCommand(this.GetWalletPath);

        public ICommand ConfirmCommand => new RelayCommand(this.Confirm);
        #endregion

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public event EventHandler<OpenWalletDialogResult> SetDialogResultAndClose;

        public OpenWalletDialogResult DialogResult { get; private set; }
        #endregion

        #region Public Methods 
        public void UpdatePassword(string updatedPassword)
        {
            this.password = updatedPassword;

            // Update dependent property
            RaisePropertyChanged(nameof(this.ConfirmEnabled));
        }
        #endregion

        #region Private Methods 

        private void GetWalletPath()
        {
            // TODO Localise file filter text
            var path = this.fileDialogService.OpenFileDialog("NEP-6 Wallet|*.json|SQLite Wallet|*.db3");

            if (string.IsNullOrEmpty(path)) return;

            this.WalletPath = path;
        }

        private void Confirm()
        {
            if (!this.ConfirmEnabled) return;

            if (this.SetDialogResultAndClose == null) return;

            var dialogResult = new OpenWalletDialogResult(
                this.WalletPath, 
                this.password);

            this.SetDialogResultAndClose(this, dialogResult);
        }
        #endregion
    }
}