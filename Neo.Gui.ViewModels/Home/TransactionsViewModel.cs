using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Neo.Gui.Base.Collections;
using Neo.Gui.Base.Data;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Managers;
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
        private readonly IProcessHelper processHelper;
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

        public ICommand CopyTransactionIdCommand => new RelayCommand(this.CopyTransactionId);

        public ICommand ViewSelectedTransactionDetailsCommand => new RelayCommand(this.ViewSelectedTransactionDetails);
        #endregion

        #region Constructor 
        public TransactionsViewModel(
            IMessageSubscriber messageSubscriber,
            IClipboardManager clipboardManager,
            IProcessHelper processHelper,
            ISettingsManager settingsManager)
        {
            this.messageSubscriber = messageSubscriber;
            this.clipboardManager = clipboardManager;
            this.processHelper = processHelper;
            this.settingsManager = settingsManager;

            this.Transactions = new ConcurrentObservableCollection<TransactionItem>();
        }
        #endregion

        #region ILoadable implementation
        public void OnLoad(params object[] parameters)
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

        #region Private Methods 
        private void CopyTransactionId()
        {
            if (this.SelectedTransaction == null) return;

            this.clipboardManager.SetText(this.SelectedTransaction.Id);
        }

        private void ViewSelectedTransactionDetails()
        {
            if (this.SelectedTransaction == null) return;

            if (string.IsNullOrEmpty(this.SelectedTransaction.Id)) return;

            var url = string.Format(this.settingsManager.TransactionURLFormat, this.SelectedTransaction.Id.Substring(2));

            this.processHelper.OpenInExternalBrowser(url);
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