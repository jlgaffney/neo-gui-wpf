using System;
using System.Collections.ObjectModel;
using System.Linq;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Voting
{
    public class ElectionViewModel :
        ViewModelBase,
        ILoadable
    {
        private readonly IAccountService accountService;
        private readonly ILocalNodeService localNodeService;
        private readonly ITransactionService transactionService;
        private readonly IWalletService walletService;

        private string selectedPublicKey;

        public ElectionViewModel() { }
        public ElectionViewModel(
            IAccountService accountService,
            ILocalNodeService localNodeService,
            ITransactionService transactionService,
            IWalletService walletService)
        {
            this.accountService = accountService;
            this.localNodeService = localNodeService;
            this.transactionService = transactionService;
            this.walletService = walletService;

            PublicKeys = new ObservableCollection<string>();
        }

        
        public ObservableCollection<string> PublicKeys { get; }

        public string SelectedPublicKey
        {
            get => selectedPublicKey;
            set
            {
                if (Equals(selectedPublicKey, value))
                {
                    return;
                }

                selectedPublicKey = value;

                this.RaisePropertyChanged();

                this.RaisePropertyChanged(nameof(TransactionFee));
                this.RaisePropertyChanged(nameof(SubmitEnabled));
            }
        }

        public string TransactionFee
        {
            get
            {
                decimal fee;
                if (string.IsNullOrEmpty(SelectedPublicKey))
                {
                    fee = 0;
                }
                else
                {
                    // TODO Get transaction fee
                    /*var transactionParameters = new ElectionTransactionParameters(this.SelectedPublicKey);

                    fee = this.walletController.GetTransactionFee(transactionParameters);*/
                    fee = 0;
                }

                return $"{fee} GAS";
            }
        }

        public bool SubmitEnabled => SelectedPublicKey != null;

        public ReactiveCommand SubmitCommand => ReactiveCommand.Create(Submit);


        public void Load()
        {
            var accountPublicKeys = accountService.GetStandardAccounts()
                .Select(account => account.GetKey().PublicKey.ToString());

            PublicKeys.AddRange(accountPublicKeys);
        }

        private void Submit()
        {
            if (!SubmitEnabled)
            {
                return;
            }
            
            var electionTransaction = transactionService.CreateElectionTransaction(SelectedPublicKey.ToECPoint());

            if (walletService.SignTransaction(electionTransaction))
            {
                localNodeService.RelayTransaction(electionTransaction);
            }
            else
            {
                // TODO Notify user
            }

            OnClose();
        }
    }
}
