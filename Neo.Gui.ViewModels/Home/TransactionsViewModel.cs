using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Collections;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.ViewModels.Home
{
    public class TransactionsViewModel : 
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<ClearTransactionsMessage>,
        IMessageHandler<TransactionsHaveChangedMessage>
    {
        #region Private Fields 
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IClipboardManager clipboardManager;
        private readonly IProcessManager processManager;
        private readonly ISettingsManager settingsManager;

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

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.CopyTransactionIdEnabled));
            }
        }

        public bool CopyTransactionIdEnabled => this.SelectedTransaction != null;

        public RelayCommand CopyTransactionIdCommand => new RelayCommand(this.CopyTransactionId);

        public RelayCommand ViewSelectedTransactionDetailsCommand => new RelayCommand(this.ViewSelectedTransactionDetails);
        #endregion

        #region Constructor 
        public TransactionsViewModel(
            IMessageSubscriber messageSubscriber,
            IClipboardManager clipboardManager,
            IProcessManager processManager,
            ISettingsManager settingsManager)
        {
            this.messageSubscriber = messageSubscriber;
            this.clipboardManager = clipboardManager;
            this.processManager = processManager;
            this.settingsManager = settingsManager;

            this.Transactions = new ConcurrentObservableCollection<TransactionItem>();
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

        #region Private Methods 
        private void CopyTransactionId()
        {
            if (this.SelectedTransaction == null) return;

            this.clipboardManager.SetText(this.SelectedTransaction.Hash.ToString());
        }

        private void ViewSelectedTransactionDetails()
        {
            if (this.SelectedTransaction == null) return;

            if (string.IsNullOrEmpty(this.SelectedTransaction.Hash.ToString())) return;

            var url = string.Format(this.settingsManager.TransactionURLFormat, this.SelectedTransaction.Hash.ToString().Substring(2));

            this.processManager.OpenInExternalBrowser(url);
        }
        #endregion
    }
}