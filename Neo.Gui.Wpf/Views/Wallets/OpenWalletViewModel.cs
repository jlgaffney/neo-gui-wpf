using System;

using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Services;

using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public class OpenWalletViewModel : ViewModelBase, IDialogViewModel<OpenWalletDialogResult>
    {
        #region Private Fields
        private readonly IFileDialogService fileDialogService;

        private string walletPath;
        private string password;
        private bool repairMode;
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

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.ConfirmEnabled));
            }
        }

        public bool RepairMode
        {
            get => this.repairMode;
            set
            {
                if (this.repairMode == value) return;

                this.repairMode = value;

                NotifyPropertyChanged();
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

        public RelayCommand GetWalletPathCommand => new RelayCommand(this.GetWalletPath);

        public RelayCommand ConfirmCommand => new RelayCommand(this.Confirm);
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
            NotifyPropertyChanged(nameof(this.ConfirmEnabled));
        }
        #endregion

        #region Private Methods 

        private void GetWalletPath()
        {
            var walletFilePath = this.fileDialogService.OpenFileDialog("db3", "Wallet File|*.db3");

            if (string.IsNullOrEmpty(walletFilePath)) return;

            this.WalletPath = walletFilePath;
        }

        private void Confirm()
        {
            if (!this.ConfirmEnabled) return;

            if (this.SetDialogResultAndClose == null) return;

            var dialogResult = new OpenWalletDialogResult(
                this.WalletPath, 
                this.password, 
                this.RepairMode);

            this.SetDialogResultAndClose(this, dialogResult);
        }
        #endregion
    }
}