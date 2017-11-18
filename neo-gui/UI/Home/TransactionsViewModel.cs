using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Neo.Core;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Properties;
using Neo.UI.Base.Collections;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Home
{
    public class TransactionsViewModel : ViewModelBase
    {
        private readonly IDispatcher dispatcher;

        private TransactionItem selectedTransaction;

        public TransactionsViewModel(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

            this.Transactions = new ConcurrentObservableCollection<TransactionItem>();
        }

        public ConcurrentObservableCollection<TransactionItem> Transactions { get; }

        public TransactionItem SelectedTransaction
        {
            get => this.selectedTransaction;
            set
            {
                if (this.selectedTransaction == value) return;

                this.selectedTransaction = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.CopyTransactionIdEnabled));
            }
        }

        public bool CopyTransactionIdEnabled => this.SelectedTransaction != null;
        
        public ICommand CopyTransactionIdCommand => new RelayCommand(this.CopyTransactionId);

        public ICommand ViewSelectedTransactionDetailsCommand => new RelayCommand(this.ViewSelectedTransactionDetails);

        private void ViewSelectedTransactionDetails()
        {
            if (this.SelectedTransaction == null) return;

            if (string.IsNullOrEmpty(this.SelectedTransaction.Id)) return;

            var url = string.Format(Settings.Default.Urls.TransactionUrl, this.SelectedTransaction.Id.Substring(2));

            Process.Start(url);
        }
        
        private void CopyTransactionId()
        {
            if (this.SelectedTransaction == null) return;

            Clipboard.SetDataObject(this.SelectedTransaction.Id);
        }

        public void UpdateTransactions(IEnumerable<TransactionInfo> transactions)
        {
            this.dispatcher.InvokeOnMainUIThread(() =>
            {
                // Update transaction list
                foreach (var transactionInfo in transactions)
                {
                    var transactionItem = new TransactionItem
                    {
                        Info = transactionInfo
                    };

                    var transactionIndex = this.GetTransactionIndex(transactionItem.Id);

                    // Check transaction exists in list
                    if (transactionIndex >= 0)
                    {
                        // Update transaction info
                        this.Transactions.Replace(transactionIndex, transactionItem);
                    }
                    else
                    {
                        // Add transaction to list
                        this.Transactions.Insert(0, transactionItem);
                    }
                }

                // Update transaction confirmations
                var transactionList = this.Transactions.ConvertToList();
                foreach (var transactionItem in transactionList)
                {
                    uint transactionHeight = 0;

                    if (transactionItem.Info?.Height != null)
                    {
                        transactionHeight = transactionItem.Info.Height.Value;
                    }

                    var confirmations = ((int) Blockchain.Default.Height) - ((int) transactionHeight) + 1;

                    transactionItem.SetConfirmations(confirmations);
                }
            });
        }

        private int GetTransactionIndex(string transactionId)
        {
            var translationList = this.Transactions.ConvertToList();

            for (int i = 0; i < translationList.Count; i++)
            {
                if (translationList[i].Id == transactionId) return i;
            }

            // Could not find transaction
            return -1;
        }
    }
}