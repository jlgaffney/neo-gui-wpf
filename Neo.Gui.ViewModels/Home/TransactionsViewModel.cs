using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.ViewModels.Data;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Wallet.Messages;

namespace Neo.Gui.ViewModels.Home
{
    public class TransactionsViewModel : 
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<TransactionAddedMessage>,
        IMessageHandler<WalletStatusMessage>
    {
        #region Private Fields 
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IClipboardManager clipboardManager;
        private readonly IProcessManager processManager;
        private readonly ISettingsManager settingsManager;

        private UiTransactionSummary selectedTransaction;
        #endregion

        #region Public Properties 
        public ObservableCollection<UiTransactionSummary> Transactions { get; }

        public UiTransactionSummary SelectedTransaction
        {
            get => this.selectedTransaction;
            set
            {
                if (Equals(this.selectedTransaction, value)) return;

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

            this.Transactions = new ObservableCollection<UiTransactionSummary>();
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
        public void HandleMessage(CurrentWalletHasChangedMessage message)
        {
            this.Transactions.Clear();
        }

        public void HandleMessage(WalletStatusMessage message)
        {
            var blockHeight = message.BlockchainStatus.Height;

            foreach (var transaction in this.Transactions)
            {
                transaction.ConfirmationsValue = blockHeight - transaction.Height + 1;
            }
        }

        public void HandleMessage(TransactionAddedMessage message)
        {
            var newTransaction = new UiTransactionSummary(message.TransactionId, message.TransactionTime, message.TransactionHeight, message.TransactionType);

            this.Transactions.Insert(0, newTransaction);
        }
        #endregion

        #region Private Methods 
        private void CopyTransactionId()
        {
            if (this.SelectedTransaction == null) return;

            this.clipboardManager.SetText(this.SelectedTransaction.Id);
        }

        private void ViewSelectedTransactionDetails()
        {
            if (this.SelectedTransaction == null) return;

            var transactionId = this.SelectedTransaction.Id;

            if (transactionId.StartsWith("0x"))
            {
                transactionId = transactionId.Substring(2);
            }

            var url = string.Format(this.settingsManager.TransactionURLFormat, transactionId);

            this.processManager.OpenInExternalBrowser(url);
        }
        #endregion
    }
}