using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Data;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Messages;
using Neo.UI.Core.Messaging.Interfaces;

namespace Neo.Gui.ViewModels.Home
{
    public class TransactionsViewModel : 
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<CurrentWalletHasChangedMessage>,
        IMessageHandler<TransactionAddedMessage>,
        IMessageHandler<TransactionConfirmationsUpdatedMessage>
    {
        #region Private Fields 
        private readonly IMessageSubscriber messageSubscriber;
        private readonly IClipboardManager clipboardManager;
        private readonly IProcessManager processManager;
        private readonly ISettingsManager settingsManager;

        private TransactionItem selectedTransaction;
        #endregion

        #region Public Properties 
        public ObservableCollection<TransactionItem> Transactions { get; }

        public TransactionItem SelectedTransaction
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

            this.Transactions = new ObservableCollection<TransactionItem>();
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

        public void HandleMessage(TransactionConfirmationsUpdatedMessage message)
        {
            var blockHeight = message.BlockHeight;

            foreach (var transactionItem in this.Transactions)
            {
                transactionItem.Confirmations = blockHeight - transactionItem.Height + 1;
            }
        }

        public void HandleMessage(TransactionAddedMessage message)
        {
            this.Transactions.Insert(0, message.Transaction);
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