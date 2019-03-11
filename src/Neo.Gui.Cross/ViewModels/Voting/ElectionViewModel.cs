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

        private string selectedPublicKey;

        public ElectionViewModel() { }
        public ElectionViewModel(
            IAccountService accountService)
        {
            this.accountService = accountService;

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

        private async void Submit()
        {
            if (!SubmitEnabled)
            {
                return;
            }

            // TODO Build transaction, sign it, and relay transaction to the network

            /*var transactionParameters = new ElectionTransactionParameters(this.SelectedPublicKey);

            await this.walletController.BuildSignAndRelayTransaction(transactionParameters);*/
            
            OnClose();
        }
    }
}
