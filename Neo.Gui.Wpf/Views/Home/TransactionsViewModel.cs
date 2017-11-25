using System.Windows;
using System.Windows.Input;
using Neo.Gui.Base.Collections;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Messaging;
using Neo.Gui.Base.MVVM;
using Neo.Gui.Wpf.Messages;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Home
{
    public class TransactionsViewModel : 
        ViewModelBase,
        ILoadable,
        IMessageHandler<ClearTransactionsMessage>,
        IMessageHandler<TransactionsHaveChangedMessage>
    {
        #region Private Fields 
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IExternalProcessHelper externalProcessHelper;

        private TransactionItem selectedTransaction;
        #endregion

        #region Public Properties 
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
        #endregion

        #region Constructor 
        public TransactionsViewModel(
            IMessageSubscriber messageSubscriber,
            IExternalProcessHelper externalProcessHelper)
        {
            this.messageSubscriber = messageSubscriber;
            this.externalProcessHelper = externalProcessHelper;

            this.Transactions = new ConcurrentObservableCollection<TransactionItem>();
        }
        #endregion

        #region ILoadable implementation

        public void OnLoad()
        {
            this.messageSubscriber.Subscribe(this);
        }
        #endregion

        #region Private Methods 
        private void CopyTransactionId()
        {
            if (this.SelectedTransaction == null) return;

            Clipboard.SetDataObject(this.SelectedTransaction.Id);
        }

        private void ViewSelectedTransactionDetails()
        {
            if (this.SelectedTransaction == null) return;

            if (string.IsNullOrEmpty(this.SelectedTransaction.Id)) return;

            var url = string.Format(Properties.Settings.Default.Urls.TransactionUrl, this.SelectedTransaction.Id.Substring(2));

            this.externalProcessHelper.OpenInExternalBrowser(url);
        }
        #endregion

        #region IMessageHandler Implementation 
        public void HandleMessage(ClearTransactionsMessage message)
        {
            this.Transactions.Clear();
        }

        public void HandleMessage(TransactionsHaveChangedMessage message)
        {
            this.Transactions.Clear();

            foreach (var transaction in message.Transactions)
            {
                this.Transactions.Add(transaction);
            }
        }
        #endregion
    }
}