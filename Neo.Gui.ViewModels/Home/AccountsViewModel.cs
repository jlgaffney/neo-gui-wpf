using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Base.Dialogs.LoadParameters.Voting;
using Neo.Gui.Base.Dialogs.Results.Wallets;
using Neo.Gui.Base.Dialogs.Results.Voting;
using Neo.Gui.Base.Helpers;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;

using Neo.Gui.Globalization.Resources;

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

        public bool WalletIsOpen => this.walletController.WalletIsOpen;

        public bool ViewPrivateKeyEnabled => this.SelectedAccount != null && this.SelectedAccount.Type == AccountType.Standard;

        public bool ViewContractEnabled => this.SelectedAccount != null && this.SelectedAccount.Type != AccountType.WatchOnly;

        public bool ShowVotingDialogEnabled => this.SelectedAccount != null && this.SelectedAccount.Type != AccountType.WatchOnly && this.SelectedAccount.Neo > Fixed8.Zero;

        public bool CopyAddressToClipboardEnabled => this.SelectedAccount != null;

        public bool DeleteAccountEnabled => this.SelectedAccount != null;

        #endregion Properties

        #region Commands
        public ICommand CreateNewAddressCommand => new RelayCommand(this.CreateNewAccount);

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
            RaisePropertyChanged(nameof(this.WalletIsOpen));
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
        public void OnLoad()
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
        private void CreateNewAccount()
        {
            this.walletController.CreateNewAccount();
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
            if (!this.ViewPrivateKeyEnabled) return;

            var selectedAccountKey = this.walletController.GetAccountKey(this.SelectedAccount.ScriptHash);
            var selectedAccountScriptHash = this.SelectedAccount.ScriptHash;

            this.dialogManager.ShowDialog<ViewPrivateKeyDialogResult, ViewPrivateKeyLoadParameters>(
                new ViewPrivateKeyLoadParameters(selectedAccountScriptHash, selectedAccountKey));
        }

        private void ViewContract()
        {
            if (!this.ViewContractEnabled) return;

            var selectedAccountContract = this.walletController.GetAccountContract(this.SelectedAccount.ScriptHash);

            this.dialogManager.ShowDialog<ViewContractDialogResult, ViewContractLoadParameters>(
                new ViewContractLoadParameters(selectedAccountContract));
        }

        private void ShowVotingDialog()
        {
            if (!this.ShowVotingDialogEnabled) return;

            this.dialogManager.ShowDialog<VotingDialogResult, VotingLoadParameters>(
                new VotingLoadParameters(this.SelectedAccount.ScriptHash));
        }

        private void CopyAddressToClipboard()
        {
            if (this.SelectedAccount == null) return;

            var selectedAccountAddress = this.walletController.ToAddress(this.SelectedAccount.ScriptHash);

            this.clipboardManager.SetText(selectedAccountAddress);
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

            var deletedSuccessfully = this.walletController.DeleteAccount(accountToDelete);

            if (!deletedSuccessfully)
            {
                // TODO Show error message

                return;
            }

            this.Accounts.Remove(accountToDelete);
        }

        private void ViewSelectedAccountDetails()
        {
            if (this.SelectedAccount == null) return;

            var selectedAccountAddress = this.walletController.ToAddress(this.SelectedAccount.ScriptHash);

            var url = string.Format(this.settingsManager.AddressURLFormat, selectedAccountAddress);
            
            this.processHelper.OpenInExternalBrowser(url);
        }
        #endregion Account Menu Command Methods
    }
}