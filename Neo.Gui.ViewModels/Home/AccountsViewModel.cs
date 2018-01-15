using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Globalization.Resources;
using Neo.Gui.Dialogs;
using Neo.Gui.Dialogs.LoadParameters.Accounts;
using Neo.Gui.Dialogs.LoadParameters.Voting;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Data;
using Neo.UI.Core.Extensions;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Messages;
using Neo.UI.Core.Messaging.Interfaces;

namespace Neo.Gui.ViewModels.Home
{
    public class AccountsViewModel : 
        ViewModelBase, 
        ILoadable,
        IUnloadable,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<AccountAddedMessage>
    {
        #region Private Fields 
        private readonly IWalletController walletController;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IClipboardManager clipboardManager;
        private readonly IProcessManager processManager;
        private readonly IDialogManager dialogManager;
        private readonly ISettingsManager settingsManager;

        private AccountItem selectedAccount;
        #endregion

        #region Public Properties
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

        public RelayCommand CreateNewAddressCommand => new RelayCommand(this.CreateNewAccount);

        public RelayCommand ImportWifPrivateKeyCommand => new RelayCommand(this.ImportWifPrivateKey);

        public RelayCommand ImportFromCertificateCommand => new RelayCommand(this.ImportCertificate);

        public RelayCommand ImportWatchOnlyAddressCommand => new RelayCommand(this.ImportWatchOnlyAddress);

        public RelayCommand CreateMultiSignatureContractAddressCommand => new RelayCommand(this.CreateMultiSignatureContract);

        public RelayCommand CreateLockContractAddressCommand => new RelayCommand(this.CreateLockAddress);

        public RelayCommand CreateCustomContractAddressCommand => new RelayCommand(this.ImportCustomContract);

        public RelayCommand ViewPrivateKeyCommand => new RelayCommand(this.ViewPrivateKey);

        public RelayCommand ViewContractCommand => new RelayCommand(this.ViewContract);

        public RelayCommand ShowVotingDialogCommand => new RelayCommand(this.ShowVotingDialog);

        public RelayCommand CopyAddressToClipboardCommand => new RelayCommand(this.CopyAddressToClipboard);

        public RelayCommand DeleteAccountCommand => new RelayCommand(this.DeleteAccount);

        public RelayCommand ViewSelectedAccountDetailsCommand => new RelayCommand(this.ViewSelectedAccountDetails);
        #endregion Command
        
        #region Constructor 
        public AccountsViewModel(
            IWalletController walletController,
            IMessageSubscriber messageSubscriber, 
            IDialogManager dialogManager,
            IClipboardManager clipboardManager,
            IProcessManager processManager,
            ISettingsManager settingsManager)
        {
            this.walletController = walletController;
            this.messageSubscriber = messageSubscriber;
            this.dialogManager = dialogManager;
            this.clipboardManager = clipboardManager;
            this.processManager = processManager;
            this.settingsManager = settingsManager;

            this.Accounts = new ObservableCollection<AccountItem>();
        }
        #endregion

        #region IMessageHandler implementation 
        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            this.Accounts.Clear();

            RaisePropertyChanged(nameof(this.WalletIsOpen));
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
            this.walletController.CreateAccount();
        }

        private void ImportWifPrivateKey()
        {
            this.dialogManager.ShowDialog<ImportPrivateKeyLoadParameters>();
        }

        private void ImportCertificate()
        {
            this.dialogManager.ShowDialog<ImportCertificateLoadParameters>();
        }

        private void ImportWatchOnlyAddress()
        {
            var addresses = this.dialogManager.ShowInputDialog(Strings.ImportWatchOnlyAddress, Strings.Address);

            if (string.IsNullOrEmpty(addresses)) return;

            var addressArray = addresses.ToLines();

            this.walletController.ImportWatchOnlyAddress(addressArray);
        }

        private void CreateMultiSignatureContract()
        {
            this.dialogManager.ShowDialog<CreateMultiSigContractLoadParameters>();
        }

        private void CreateLockAddress()
        {
            this.dialogManager.ShowDialog<CreateLockAccountLoadParameters>();
        }

        private void ImportCustomContract()
        {
            this.dialogManager.ShowDialog<ImportCustomContractLoadParameters>();
        }

        private void ViewPrivateKey()
        {
            if (!this.ViewPrivateKeyEnabled) return;
            
            this.dialogManager.ShowDialog(new ViewPrivateKeyLoadParameters(this.SelectedAccount.ScriptHash));
        }

        private void ViewContract()
        {
            if (!this.ViewContractEnabled) return;

            this.dialogManager.ShowDialog(new ViewContractLoadParameters(this.SelectedAccount.ScriptHash.ToString()));
        }

        private void ShowVotingDialog()
        {
            if (!this.ShowVotingDialogEnabled) return;

            this.dialogManager.ShowDialog(new VotingLoadParameters(this.SelectedAccount.ScriptHash));
        }

        private void CopyAddressToClipboard()
        {
            if (this.SelectedAccount == null) return;

            var selectedAccountAddress = this.walletController.ScriptHashToAddress(this.SelectedAccount.ScriptHash);

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
                // TODO Show error message and create UnitTest for this feature.

                return;
            }

            this.Accounts.Remove(accountToDelete);
        }

        private void ViewSelectedAccountDetails()
        {
            if (this.SelectedAccount == null) return;

            var selectedAccountAddress = this.walletController.ScriptHashToAddress(this.SelectedAccount.ScriptHash);

            var url = string.Format(this.settingsManager.AddressURLFormat, selectedAccountAddress);
            
            this.processManager.OpenInExternalBrowser(url);
        }
        #endregion 
    }
}