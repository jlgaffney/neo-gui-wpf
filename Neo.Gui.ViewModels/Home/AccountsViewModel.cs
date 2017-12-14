using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Base.Globalization;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Dialogs.LoadParameters.Voting;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Dialogs.Results.Voting;

namespace Neo.Gui.ViewModels.Home
{
    public class AccountsViewModel : 
        ViewModelBase, 
        ILoadable,
        IUnloadable,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<ClearAccountsMessage>,
        IMessageHandler<AccountAddedMessage>
    {
        #region Private Fields 
        private readonly IWalletController walletController;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IClipboardManager clipboardManager;
        private readonly IProcessHelper processHelper;
        private readonly IDialogManager dialogManager;
        private readonly ISettingsManager settingsManager;

        private AccountItem selectedAccount;
        #endregion

        #region Properties
        public Action NotifyBalanceChangedAction { get; set; }

        public ObservableCollection<AccountItem> Accounts { get; }

        public AccountItem SelectedAccount
        {
            get => this.selectedAccount;
            set
            {
                if (this.selectedAccount == value) return;

                this.selectedAccount = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.ViewPrivateKeyEnabled));
                RaisePropertyChanged(nameof(this.ViewContractEnabled));
                RaisePropertyChanged(nameof(this.ShowVotingDialogEnabled));
                RaisePropertyChanged(nameof(this.CopyAddressToClipboardEnabled));
                RaisePropertyChanged(nameof(this.DeleteAccountEnabled));
            }
        }

        public bool MenuItemsEnabled => this.walletController.WalletIsOpen;

        public bool ViewPrivateKeyEnabled =>
            this.SelectedAccount != null &&
            this.SelectedAccount.Contract != null &&
            this.SelectedAccount.Contract.IsStandard;

        public bool ViewContractEnabled =>
            this.SelectedAccount != null &&
            this.SelectedAccount.Contract != null;

        public bool ShowVotingDialogEnabled =>
            this.SelectedAccount != null &&
            this.SelectedAccount.Contract != null &&
            this.SelectedAccount.Neo > Fixed8.Zero;

        public bool CopyAddressToClipboardEnabled => this.SelectedAccount != null;

        public bool DeleteAccountEnabled => this.SelectedAccount != null;

        #endregion Properties

        #region Commands
        public ICommand CreateNewAddressCommand => new RelayCommand(this.CreateNewKey);

        public ICommand ImportWifPrivateKeyCommand => new RelayCommand(this.ImportWifPrivateKey);
        public ICommand ImportFromCertificateCommand => new RelayCommand(this.ImportCertificate);

        public ICommand ImportWatchOnlyAddressCommand => new RelayCommand(this.ImportWatchOnlyAddress);

        public ICommand CreateMultiSignatureContractAddressCommand => new RelayCommand(this.CreateMultiSignatureContract);
        public ICommand CreateLockContractAddressCommand => new RelayCommand(this.CreateLockAddress);

        public ICommand CreateCustomContractAddressCommand => new RelayCommand(this.ImportCustomContract);

        public ICommand ViewPrivateKeyCommand => new RelayCommand(this.ViewPrivateKey);
        public ICommand ViewContractCommand => new RelayCommand(this.ViewContract);
        public ICommand ShowVotingDialogCommand => new RelayCommand(this.ShowVotingDialog);
        public ICommand CopyAddressToClipboardCommand => new RelayCommand(this.CopyAddressToClipboard);
        public ICommand DeleteAccountCommand => new RelayCommand(this.DeleteAccount);

        public ICommand ViewSelectedAccountDetailsCommand => new RelayCommand(this.ViewSelectedAccountDetails);
        #endregion Command
        
        #region Constructor 
        public AccountsViewModel(
            IWalletController walletController,
            IMessageSubscriber messageSubscriber, 
            IDialogManager dialogManager,
            IClipboardManager clipboardManager,
            IProcessHelper processHelper,
            ISettingsManager settingsManager)
        {
            this.walletController = walletController;
            this.messageSubscriber = messageSubscriber;
            this.dialogManager = dialogManager;
            this.clipboardManager = clipboardManager;
            this.processHelper = processHelper;
            this.settingsManager = settingsManager;

            this.Accounts = new ObservableCollection<AccountItem>();
        }
        #endregion

        #region IMessageHandler implementation 
        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            RaisePropertyChanged(nameof(this.MenuItemsEnabled));
        }

        public void HandleMessage(ClearAccountsMessage message)
        {
            this.Accounts.Clear();
        }

        public void HandleMessage(AccountAddedMessage message)
        {
            this.Accounts.Add(message.Account);
        }
        #endregion

        #region ILoadable implementation 
        public void OnLoad(params object[] parameters)
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion

        #region IUnloadable implementation
        public void OnUnload()
        {
            this.messageSubscriber.Unsubscribe(this);
        }
        #endregion

        #region Private Methods 
        private void CreateNewKey()
        {
            this.walletController.CreateNewKey();
        }

        private void ImportWifPrivateKey()
        {
            this.dialogManager.ShowDialog<ImportPrivateKeyDialogResult>();
        }

        private void ImportCertificate()
        {
            this.dialogManager.ShowDialog<ImportCertificateDialogResult>();
        }

        private void ImportWatchOnlyAddress()
        {
            var address = this.dialogManager.ShowInputDialog(Strings.ImportWatchOnlyAddress, Strings.Address);

            if (string.IsNullOrEmpty(address)) return;

            this.walletController.ImportWatchOnlyAddress(address);
        }

        private void CreateMultiSignatureContract()
        {
            this.dialogManager.ShowDialog<CreateMultiSigContractDialogResult>();
        }

        private void CreateLockAddress()
        {
            this.dialogManager.ShowDialog<CreateLockAccountDialogResult>();
        }

        private void ImportCustomContract()
        {
            this.dialogManager.ShowDialog<ImportCustomContractDialogResult>();
        }

        private void ViewPrivateKey()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var contract = this.SelectedAccount.Contract;
            var key = this.walletController.GetKeyByScriptHash(contract.ScriptHash);

            this.dialogManager.ShowDialog<ViewPrivateKeyDialogResult, ViewPrivateKeyLoadParameters>(
                new LoadParameters<ViewPrivateKeyLoadParameters>(new ViewPrivateKeyLoadParameters(key, contract.ScriptHash)));
        }

        private void ViewContract()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var contract = this.SelectedAccount.Contract;

            this.dialogManager.ShowDialog<ViewContractDialogResult, ViewContractLoadParameters>(
                new LoadParameters<ViewContractLoadParameters>(new ViewContractLoadParameters(contract)));
        }

        private void ShowVotingDialog()
        {
            if (this.SelectedAccount?.Contract == null) return;

            this.dialogManager.ShowDialog<VotingDialogResult, VotingLoadParameters>(
                new LoadParameters<VotingLoadParameters>(new VotingLoadParameters(this.SelectedAccount.Contract.ScriptHash)));
        }

        private void CopyAddressToClipboard()
        {
            if (this.SelectedAccount == null) return;

            this.clipboardManager.SetText(this.SelectedAccount.Address);
        }

        private void DeleteAccount()
        {
            if (this.SelectedAccount == null) return;

            var accountToDelete = this.SelectedAccount;

            var result = this.dialogManager.ShowMessageDialog(
                Strings.DeleteAddressConfirmationCaption,
                Strings.DeleteAddressConfirmationMessage,
                MessageDialogType.YesNo,
                MessageDialogResult.No);

            if (result != MessageDialogResult.Yes) return;

            this.walletController.DeleteAccount(accountToDelete);
            this.Accounts.Remove(accountToDelete);
        }

        private void ViewSelectedAccountDetails()
        {
            if (this.SelectedAccount == null) return;
            
            var url = string.Format(this.settingsManager.AddressURLFormat, this.SelectedAccount.Address);
            
            this.processHelper.OpenInExternalBrowser(url);
        }
        #endregion Account Menu Command Methods
    }
}