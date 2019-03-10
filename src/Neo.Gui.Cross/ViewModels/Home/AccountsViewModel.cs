using System.Collections.ObjectModel;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Messaging;
using Neo.Gui.Cross.Models;
using Neo.Gui.Cross.Services;
using Neo.Gui.Cross.ViewModels.Accounts;
using Neo.Gui.Cross.ViewModels.Voting;
using Neo.Ledger;
using Neo.Wallets;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Home
{
    public class AccountsViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<WalletOpenedMessage>,
        IMessageHandler<WalletClosedMessage>,
        IMessageHandler<AccountBalancesChangedMessage>
    {
        private readonly IAccountService accountService;
        private readonly IAccountBalanceService accountBalanceService;
        private readonly IClipboardService clipboardService;
        private readonly IMessageAggregator messageAggregator;
        private readonly IWalletService walletService;
        private readonly IWindowService windowService;

        private AccountSummary selectedAccount;

        public AccountsViewModel() { }
        public AccountsViewModel(
            IAccountService accountService,
            IAccountBalanceService accountBalanceService,
            IClipboardService clipboardService,
            IMessageAggregator messageAggregator,
            IWalletService walletService,
            IWindowService windowService)
        {
            this.accountService = accountService;
            this.accountBalanceService = accountBalanceService;
            this.clipboardService = clipboardService;
            this.messageAggregator = messageAggregator;
            this.walletService = walletService;
            this.windowService = windowService;

            Accounts = new ObservableCollection<AccountSummary>();
        }

        public ObservableCollection<AccountSummary> Accounts { get; }

        public AccountSummary SelectedAccount
        {
            get => selectedAccount;
            set
            {
                if (selectedAccount == value)
                {
                    return;
                }

                selectedAccount = value;

                this.RaisePropertyChanged();
            }
        }

        public bool ViewPrivateKeyEnabled => SelectedAccount != null && SelectedAccount.Type == AccountType.Standard;

        public bool ViewContractEnabled => SelectedAccount != null && SelectedAccount.Type != AccountType.WatchOnly;

        public bool ShowVotingDialogEnabled => SelectedAccount != null && SelectedAccount.Type != AccountType.WatchOnly && SelectedAccount.NeoBalance > 0;

        public ReactiveCommand CreateNewAccountCommand => ReactiveCommand.Create(CreateNewAccount);

        public ReactiveCommand ImportPrivateKeyCommand => ReactiveCommand.Create(() => windowService.ShowDialog<ImportPrivateKeyViewModel>());

        public ReactiveCommand ImportCertificateCommand => ReactiveCommand.Create(() => windowService.ShowDialog<ImportCertificateViewModel>());

        //public ReactiveCommand ImportWatchOnlyAddressCommand => ReactiveCommand.Create(() => windowService.ShowDialog<ImportWatchOnlyAddressViewModel>());

        public ReactiveCommand CreateMultiSignatureContractCommand => ReactiveCommand.Create(() => windowService.ShowDialog<CreateMultiSignatureContractViewModel>());

        public ReactiveCommand CreateLockContractCommand => ReactiveCommand.Create(() => windowService.ShowDialog<CreateLockAccountViewModel>());

        //public ReactiveCommand CreateCustomContractCommand => ReactiveCommand.Create(() => windowService.ShowDialog<CreateCustomContractViewModel>());

        public ReactiveCommand ViewPrivateKeyCommand => ReactiveCommand.Create(() => windowService.ShowDialog<ViewPrivateKeyViewModel>());

        public ReactiveCommand ViewContractCommand => ReactiveCommand.Create(() => windowService.ShowDialog<ViewContractViewModel>());

        public ReactiveCommand VoteCommand => ReactiveCommand.Create(() => windowService.ShowDialog<VotingViewModel>());

        public ReactiveCommand CopyAddressToClipboardCommand => ReactiveCommand.Create(CopyAddressToClipboard);

        public ReactiveCommand DeleteAccountCommand => ReactiveCommand.Create(DeleteSelectedAccount);

        public void Load()
        {
            LoadAccounts();

            messageAggregator.Subscribe(this);
        }

        public void Unload()
        {
            messageAggregator.Unsubscribe(this);
        }

        public void HandleMessage(WalletOpenedMessage message)
        {
            LoadAccounts();
        }

        public void HandleMessage(WalletClosedMessage message)
        {
            Accounts.Clear();
        }

        public void HandleMessage(AccountBalancesChangedMessage message)
        {
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            Accounts.Clear();

            if (!walletService.WalletIsOpen)
            {
                return;
            }

            foreach (var account in accountService.GetAllAccounts())
            {
                var globalAssetBalances = accountBalanceService.GetGlobalAssetBalances(account.ScriptHash);
                
                if (!globalAssetBalances.TryGetValue(Blockchain.GoverningToken.Hash, out var neoBalance))
                {
                    neoBalance = Fixed8.Zero;
                }
                
                if (!globalAssetBalances.TryGetValue(Blockchain.UtilityToken.Hash, out var gasBalance))
                {
                    gasBalance = Fixed8.Zero;
                }
                
                Accounts.Add(new AccountSummary
                {
                    Label = account.Label,
                    Address = account.Address,
                    Type = account.GetAccountType(),
                    NeoBalance = (uint) ((decimal) neoBalance),
                    GasBalance = (double) ((decimal) gasBalance)
                });
            }
        }

        private void CreateNewAccount()
        {
            if (!walletService.WalletIsOpen)
            {
                return;
            }

            var newAccount = accountService.CreateStandardAccount();

            Accounts.Add(new AccountSummary
            {
                Label = "New Account", // TODO What should this be?
                Address = newAccount.Address,
                Type = newAccount.GetAccountType(),
                NeoBalance = 0,
                GasBalance = 0.0
            });
        }

        private void DeleteSelectedAccount()
        {
            if (SelectedAccount == null)
            {
                return;
            }

            // TODO Confirm deletion
            
            if (!accountService.DeleteAccount(SelectedAccount.Address.ToScriptHash()))
            {
                // TODO Notify user deletion failed
                return;
            }

            var deletedAccount = SelectedAccount;

            SelectedAccount = null;
            Accounts.Remove(deletedAccount);
        }

        private void CopyAddressToClipboard()
        {
            if (SelectedAccount == null)
            {
                return;
            }

            clipboardService.SetText(SelectedAccount.Address);
        }
    }
}
