using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

using MahApps.Metro.Controls.Dialogs;
using Neo.Core;
using Neo.Properties;
using Neo.UI.Accounts;
using Neo.UI.Base.Collections;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.Helpers;
using Neo.UI.Base.Messages;
using Neo.UI.Base.MVVM;
using Neo.UI.Contracts;
using Neo.UI.Messages;
using Neo.UI.Voting;
using Neo.Wallets;

namespace Neo.UI.Home
{
    public class AccountsViewModel : 
        ViewModelBase, 
        ILoadable, 
        IMessageHandler<AccountBalancesChangedMessage>,
        IMessageHandler<EnableMenuItemsMessage>,
        IMessageHandler<ClearAccountsMessage>,
        IMessageHandler<LoadWalletAddressesMessage>,
        IMessageHandler<RestoreContractsMessage>
    {
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IMessagePublisher messagePublisher;
        private readonly IDispatcher dispatcher;

        private AccountItem selectedAccount;
        
        public AccountsViewModel(
            IMessageSubscriber messageSubscriber, 
            IMessagePublisher messagePublisher, 
            IDispatcher dispatcher)
        {
            this.messageSubscriber = messageSubscriber;
            this.messagePublisher = messagePublisher;
            this.dispatcher = dispatcher;

            this.Accounts = new ConcurrentObservableCollection<AccountItem>();
        }

        #region Properties
        public Action NotifyBalanceChangedAction { get; set; }

        public ConcurrentObservableCollection<AccountItem> Accounts { get; }

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

        public bool MenuItemsEnabled => ApplicationContext.Instance.CurrentWallet != null;

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

        public AccountItem GetAccount(string address)
        {
            return this.Accounts.FirstOrDefault(account => account.Address == address);
        }


        private async void AddAddress(UInt160 scriptHash, bool selected = false)
        {
            var address = Wallet.ToAddress(scriptHash);
            var item = this.GetAccount(address);

            if (item == null)
            {
                item = new AccountItem
                {
                    Address = address,
                    Type = AccountType.WatchOnly,
                    Neo = Fixed8.Zero,
                    Gas = Fixed8.Zero,
                    ScriptHash = scriptHash
                };

                await this.dispatcher.InvokeOnMainUIThread(() => this.Accounts.Add(item));
            }

            this.SelectedAccount = selected ? item : null;
        }

        private async void AddContract(VerificationContract contract, bool selected = false)
        {
            var item = this.GetAccount(contract.Address);

            if (item?.ScriptHash != null)
            {
                var account = item;
                await this.dispatcher.InvokeOnMainUIThread(() => this.Accounts.Remove(account));
                item = null;
            }

            if (item == null)
            {
                item = new AccountItem
                {
                    Address = contract.Address,
                    Type = contract.IsStandard ? AccountType.Standard : AccountType.NonStandard,
                    Neo = Fixed8.Zero,
                    Gas = Fixed8.Zero,
                    Contract = contract
                };

                await this.dispatcher.InvokeOnMainUIThread(() => this.Accounts.Add(item));
            }

            this.SelectedAccount = selected ? item : null;
        }

        #region Account Menu Command Methods
        private void CreateNewKey()
        {
            this.SelectedAccount = null;
            var key = ApplicationContext.Instance.CurrentWallet.CreateKey();
            foreach (var contract in ApplicationContext.Instance.CurrentWallet.GetContracts(key.PublicKeyHash))
            {
                AddContract(contract, true);
            }
        }

        private void ImportWifPrivateKey()
        {
            var view = new ImportPrivateKeyView();
            view.ShowDialog();

            var wifStrings = view.WifStrings;

            if (wifStrings == null) return;

            var wifStringList = wifStrings.ToList();

            if (!wifStringList.Any()) return;

            // Import private keys
            this.SelectedAccount = null;

            foreach (var wif in wifStringList)
            {
                KeyPair key;
                try
                {
                    key = ApplicationContext.Instance.CurrentWallet.Import(wif);
                }
                catch (FormatException)
                {
                    // Skip WIF line
                    continue;
                }
                foreach (var contract in ApplicationContext.Instance.CurrentWallet.GetContracts(key.PublicKeyHash))
                {
                    AddContract(contract, true);
                }
            }
        }

        private async void ImportCertificate()
        {
            var view = new SelectCertificateView();
            view.ShowDialog();

            if (view.SelectedCertificate == null) return;

            this.SelectedAccount = null;

            KeyPair key;
            try
            {
                key = ApplicationContext.Instance.CurrentWallet.Import(view.SelectedCertificate);
            }
            catch
            {
                await DialogCoordinator.Instance.ShowMessageAsync(this, string.Empty, "Certificate import failed!");
                return;
            }

            foreach (var contract in ApplicationContext.Instance.CurrentWallet.GetContracts(key.PublicKeyHash))
            {
                AddContract(contract, true);
            }
        }

        private void ImportWatchOnlyAddress()
        {
            if (!InputBox.Show(out var text, Strings.Address, Strings.ImportWatchOnlyAddress)) return;

            if (string.IsNullOrEmpty(text)) return;

            using (var reader = new StringReader(text))
            {
                while (true)
                {
                    var address = reader.ReadLine();
                    if (address == null) break;
                    address = address.Trim();
                    if (string.IsNullOrEmpty(address)) continue;
                    UInt160 scriptHash;
                    try
                    {
                        scriptHash = Wallet.ToScriptHash(address);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }
                    ApplicationContext.Instance.CurrentWallet.AddWatchOnly(scriptHash);
                    AddAddress(scriptHash, true);
                }
            }
        }

        private void CreateMultiSignatureContract()
        {
            var view = new CreateMultiSigContractView();
            view.ShowDialog();

            var contract = view.GetContract();

            if (contract == null) return;

            ApplicationContext.Instance.CurrentWallet.AddContract(contract);
            this.SelectedAccount = null;
            AddContract(contract, true);
        }

        private void CreateLockAddress()
        {
            var view = new CreateLockAccountView();
            view.ShowDialog();

            var contract = view.GetContract();

            if (contract == null) return;

            ApplicationContext.Instance.CurrentWallet.AddContract(contract);
            this.SelectedAccount = null;
            AddContract(contract, true);
        }

        private void ImportCustomContract()
        {
            var view = new ImportCustomContractView();
            view.ShowDialog();

            var contract = view.GetContract();

            if (contract == null) return;

            ApplicationContext.Instance.CurrentWallet.AddContract(contract);
            this.SelectedAccount = null;
            AddContract(contract, true);
        }

        private void ViewPrivateKey()
        {
            if (this.SelectedAccount?.Contract == null) return;

            var contract = this.SelectedAccount.Contract;
            var key = ApplicationContext.Instance.CurrentWallet.GetKeyByScriptHash(contract.ScriptHash);

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

            var contract = this.SelectedAccount.Contract;

            var view = new VotingView(contract.ScriptHash);
            view.ShowDialog();

            var transaction = view.GetTransaction();

            if (transaction == null) return;

            var invokeContractView = new InvokeContractView(transaction);
            invokeContractView.ShowDialog();

            transaction = invokeContractView.GetTransaction();

            if (transaction == null) return;

            TransactionHelper.SignAndShowInformation(transaction);
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

        private async void DeleteAccount()
        {
            if (this.SelectedAccount == null) return;

            var accountToDelete = this.SelectedAccount;

            if (MessageBox.Show(Strings.DeleteAddressConfirmationMessage, Strings.DeleteAddressConfirmationCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            var scriptHash = accountToDelete.ScriptHash != null
                ? accountToDelete.ScriptHash
                : accountToDelete.Contract.ScriptHash;

            ApplicationContext.Instance.CurrentWallet.DeleteAddress(scriptHash);
            await this.dispatcher.InvokeOnMainUIThread(() => this.Accounts.Remove(accountToDelete));

            this.messagePublisher.Publish(new WalletBalanceChangedMessage(true));
        }
        #endregion Account Menu Command Methods

        #region IMessageHandler implementation 
        public void HandleMessage(AccountBalancesChangedMessage message)
        {
            var coins = ApplicationContext.Instance.CurrentWallet?.GetCoins().Where(p => !p.State.HasFlag(CoinState.Spent)).ToList();

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

        public void HandleMessage(EnableMenuItemsMessage message)
        {
            this.NotifyPropertyChanged(nameof(this.MenuItemsEnabled));
        }

        public void HandleMessage(ClearAccountsMessage message)
        {
            this.Accounts.Clear();
        }

        public void HandleMessage(LoadWalletAddressesMessage message)
        {
            if (ApplicationContext.Instance.CurrentWallet == null) return;

            // Load accounts
            foreach (var scriptHash in ApplicationContext.Instance.CurrentWallet.GetAddresses())
            {
                var contract = ApplicationContext.Instance.CurrentWallet.GetContract(scriptHash);
                if (contract == null)
                {
                    this.AddAddress(scriptHash);
                }
                else
                {
                    this.AddContract(contract);
                }
            }
        }

        public void HandleMessage(RestoreContractsMessage message)
        {
            if (message.Contracts == null || !message.Contracts.Any())
            {
                return;
            }

            foreach (var contract in message.Contracts)
            {
                ApplicationContext.Instance.CurrentWallet.AddContract(contract);
                this.AddContract(contract, true);
            }
        }
        #endregion

        #region ILoadable implementation 
        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion
    }
}