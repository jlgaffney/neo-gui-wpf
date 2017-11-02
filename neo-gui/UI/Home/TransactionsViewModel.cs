using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Neo.Core;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Home
{
    public class TransactionsViewModel : ViewModelBase
    {
        private TransactionItem selectedTransaction;

        public TransactionsViewModel()
        {
            this.Transactions = new ObservableCollection<TransactionItem>();
        }

        public ObservableCollection<TransactionItem> Transactions { get; }

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

        
        private void CopyTransactionId()
        {
            if (this.SelectedTransaction == null) return;

            Clipboard.SetDataObject(this.SelectedTransaction.Id);
        }

        public void UpdateTransactions(IEnumerable<TransactionInfo> transactions)
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
                    this.Transactions[transactionIndex] = transactionItem;
                }
                else
                {
                    // Add transaction to list
                    this.Transactions.Insert(0, transactionItem);
                }
            }

            // Update transaction confirmations
            foreach (var item in this.Transactions)
            {
                uint transactionHeight = 0;

                if (item.Info != null && item.Info.Height != null)
                {
                    transactionHeight = item.Info.Height.Value;
                }

                var confirmations = ((int)Blockchain.Default.Height) - ((int)transactionHeight) + 1;

                item.SetConfirmations(confirmations);
            }
        }

        private int GetTransactionIndex(string transactionId)
        {
            for (int i = 0; i < this.Transactions.Count; i++)
            {
                if (this.Transactions[i].Id == transactionId) return i;
            }

            // Could not find transaction
            return -1;
        }
    }
}