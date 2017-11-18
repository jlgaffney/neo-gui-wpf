using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Neo.Controllers;
using Neo.Core;
using Neo.Properties;
using Neo.UI.Accounts;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Messages;
using Neo.UI.Voting;
using Neo.Wallets;

namespace Neo.UI.Home
{
    public class AccountsViewModel : 
        ViewModelBase, 
        ILoadable, 
        IMessageHandler<AccountBalancesChangedMessage>,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<ClearAccountsMessage>,
        IMessageHandler<AccountItemsChangedMessage>
    {
        #region Private Fields 
        private readonly IWalletController walletController;
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IMessagePublisher messagePublisher;

        private AccountItem selectedAccount;
        #endregion

        #region Properties
        public Action NotifyBalanceChangedAction { get; set; }

        public ObservableCollection<AccountItem> Accounts { get; private set; }

        public AccountItem SelectedAccount
        {
            get => this.selectedAccount;
            set
            {
                if (this.selectedAccount == value) return;

                this.selectedAccount = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.ViewPrivateKeyEnabled));
                NotifyPropertyChanged(nameof(this.ViewContractEnabled));
                NotifyPropertyChanged(nameof(this.ShowVotingDialogEnabled));
                NotifyPropertyChanged(nameof(this.CopyAddressToClipboardEnabled));
                NotifyPropertyChanged(nameof(this.DeleteAccountEnabled));
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
        #endregion Command

        #region Constructor 
        public AccountsViewModel(
            IWalletController walletController,
            IMessageSubscriber messageSubscriber, 
            IMessagePublisher messagePublisher)
        {
            this.walletController = walletController;
            this.messageSubscriber = messageSubscriber;
            this.messagePublisher = messagePublisher;

            this.Accounts = new ObservableCollection<AccountItem>();
        }
        #endregion

        #region IMessageHandler implementation 
        public void HandleMessage(AccountBalancesChangedMessage message)
        {
            var coins = this.walletController.GetCoins()
                .Where(p => !p.State.HasFlag(CoinState.Spent))
                .ToList();

            if (coins == null) return;

            var balanceNeo = coins.Where(p => p.Output.AssetId.Equals(Blockchain.GoverningToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));
            var balanceGas = coins.Where(p => p.Output.AssetId.Equals(Blockchain.UtilityToken.Hash)).GroupBy(p => p.Output.ScriptHash).ToDictionary(p => p.Key, p => p.Sum(i => i.Output.Value));

            var accountsList = this.Accounts.ToList();

            foreach (var account in accountsList)
            {
                var scriptHash = Wallet.ToScriptHash(account.Address);
                var neo = balanceNeo.ContainsKey(scriptHash) ? balanceNeo[scriptHash] : Fixed8.Zero;
                var gas = balanceGas.ContainsKey(scriptHash) ? balanceGas[scriptHash] : Fixed8.Zero;
                account.Neo = neo;
                account.Gas = gas;
            }
        }

        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            this.NotifyPropertyChanged(nameof(this.MenuItemsEnabled));
        }

        public void HandleMessage(ClearAccountsMessage message)
        {
            this.Accounts.Clear();
        }

        public void HandleMessage(AccountItemsChangedMessage message)
        {
            this.Accounts.Clear();
            foreach(var accountItem in message.Accounts)
            {
                this.Accounts.Add(accountItem);
            }
        }
        #endregion

        #region ILoadable implementation 
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion

        #region Private Methods 
        private void CreateNewKey()
        {
            this.walletController.CreateNewKey();
        }

        private void ImportWifPrivateKey()
        {
            var view = new ImportPrivateKeyView();
            view.ShowDialog();
        }

        private void ImportCertificate()
        {
            var view = new ImportCertificateView();
            view.ShowDialog();
        }

        private void ImportWatchOnlyAddress()
        {
            if (!InputBox.Show(out var text, Strings.Address, Strings.ImportWatchOnlyAddress)) return;

            if (string.IsNullOrEmpty(text)) return;

            this.walletController.ImportWatchOnlyAddress(text);
        }

        private void CreateMultiSignatureContract()
        {
            var view = new CreateMultiSigContractView();
            view.ShowDialog();
        }

        private void CreateLockAddress()
        {
            var view = new CreateLockAccountView();
            view.ShowDialog();
        }

        private void ImportCustomContract()
        {
            var view = new ImportCustomContractView();
            view.ShowDialog();
        }

        private void ViewPrivateKey()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var contract = this.SelectedAccount.Contract;
            var key = this.walletController.GetKeyByScriptHash(contract.ScriptHash);

            var view = new ViewPrivateKeyView(key, contract.ScriptHash);
            view.ShowDialog();
        }

        private void ViewContract()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var contract = this.SelectedAccount.Contract;

            var view = new ViewContractView(contract);
            view.ShowDialog();
        }

        private void ShowVotingDialog()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var view = new VotingView(this.SelectedAccount.Contract.ScriptHash);
            view.ShowDialog();
        }

        private void CopyAddressToClipboard()
        {
            if (this.SelectedAccount == null) return;

            try
            {
                Clipboard.SetText(this.SelectedAccount.Address);
            }
            catch (ExternalException) { }
        }

        private void DeleteAccount()
        {
            if (this.SelectedAccount == null) return;

            var accountToDelete = this.SelectedAccount;

            if (MessageBox.Show(Strings.DeleteAddressConfirmationMessage, Strings.DeleteAddressConfirmationCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            this.walletController.DeleteAccount(accountToDelete);

            this.messagePublisher.Publish(new WalletBalanceChangedMessage(true));
        }
        #endregion Account Menu Command Methods
    }
}