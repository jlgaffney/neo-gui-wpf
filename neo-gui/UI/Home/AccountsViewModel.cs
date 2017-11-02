using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

using MahApps.Metro.Controls.Dialogs;

using Neo.Properties;
using Neo.UI.Accounts;
using Neo.UI.Base.Dialogs;
using Neo.UI.Base.Helpers;
using Neo.UI.Base.MVVM;
using Neo.UI.Contracts;
using Neo.UI.Voting;
using Neo.Wallets;

namespace Neo.UI.Home
{
    public class AccountsViewModel : ViewModelBase
    {
        private readonly Action setBalanceChangedAction;

        private AccountItem selectedAccount;
        
        public AccountsViewModel(Action setBalanceChangedAction)
        {
            this.setBalanceChangedAction = setBalanceChangedAction;

            this.Accounts = new ObservableCollection<AccountItem>();
        }

        #region Properties

        public ObservableCollection<AccountItem> Accounts { get; }

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

        public void UpdateMenuItemsEnabled()
        {
            NotifyPropertyChanged(nameof(this.MenuItemsEnabled));
        }

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


        internal void AddAddress(UInt160 scriptHash, bool selected = false)
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
                    Gas = Fixed8.Zero
                };

                this.Accounts.Add(item);
            }

            this.SelectedAccount = selected ? item : null;
        }

        internal void AddContract(VerificationContract contract, bool selected = false)
        {
            var item = this.GetAccount(contract.Address);

            if (item?.ScriptHash != null)
            {
                this.Accounts.Remove(item);
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

                this.Accounts.Add(item);
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

        private void DeleteAccount()
        {
            if (this.SelectedAccount == null) return;

            var accountToDelete = this.SelectedAccount;

            if (MessageBox.Show(Strings.DeleteAddressConfirmationMessage, Strings.DeleteAddressConfirmationCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            var scriptHash = accountToDelete.ScriptHash != null
                ? accountToDelete.ScriptHash
                : accountToDelete.Contract.ScriptHash;

            ApplicationContext.Instance.CurrentWallet.DeleteAddress(scriptHash);
            this.Accounts.Remove(accountToDelete);

            this.setBalanceChangedAction();
        }

        #endregion Account Menu Command Methods
    }
}