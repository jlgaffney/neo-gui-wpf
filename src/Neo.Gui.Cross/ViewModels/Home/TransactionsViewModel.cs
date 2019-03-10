using System.Collections.ObjectModel;
using System.Linq;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Messaging;
using Neo.Gui.Cross.Models;
using Neo.Gui.Cross.Services;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels.Home
{
    public class TransactionsViewModel :
        ViewModelBase,
        ILoadable,
        IUnloadable,
        IMessageHandler<WalletOpenedMessage>,
        IMessageHandler<WalletClosedMessage>,
        IMessageHandler<BlockchainHeightChangedMessage>
    {
        private readonly IBlockchainService blockchainService;
        private readonly IClipboardService clipboardService;
        private readonly IMessageAggregator messageAggregator;
        private readonly ITransactionService transactionService;
        private readonly IWalletService walletService;

        private TransactionSummary selectedTransaction;

        public TransactionsViewModel() { }
        public TransactionsViewModel(
            IBlockchainService blockchainService,
            IClipboardService clipboardService,
            IMessageAggregator messageAggregator,
            ITransactionService transactionService,
            IWalletService walletService)
        {
            this.blockchainService = blockchainService;
            this.clipboardService = clipboardService;
            this.messageAggregator = messageAggregator;
            this.transactionService = transactionService;
            this.walletService = walletService;

            Transactions = new ObservableCollection<TransactionSummary>();
        }
        
        public ObservableCollection<TransactionSummary> Transactions { get; }
        
        public TransactionSummary SelectedTransaction
        {
            get => selectedTransaction;
            set
            {
                if (selectedTransaction == value)
                {
                    return;
                }

                selectedTransaction = value;

                this.RaisePropertyChanged();
            }
        }

        public bool TransactionIsSelected => SelectedTransaction != null;

        public ReactiveCommand CopySelectedTransactionIdCommand => ReactiveCommand.Create(CopySelectedTransactionId);

        public void Load()
        {
            LoadTransactions();

            messageAggregator.Subscribe(this);
        }

        public void Unload()
        {
            messageAggregator.Unsubscribe(this);
        }

        public void HandleMessage(WalletOpenedMessage message)
        {
            LoadTransactions();
        }

        public void HandleMessage(WalletClosedMessage message)
        {
            Transactions.Clear();
        }

        public void HandleMessage(BlockchainHeightChangedMessage message)
        {
            var transactions = Transactions.ToList();

            var blockchainHeight = blockchainService.Height;

            foreach (var transaction in transactions)
            {
                // TODO Handle unconfirmed transactions
                transaction.Confirmations = (blockchainHeight - transaction.BlockIndex + 1).ToString();
            }
        }

        private void LoadTransactions()
        {
            Transactions.Clear();

            if (!walletService.WalletIsOpen)
            {
                return;
            }

            var blockchainHeight = blockchainService.Height;

            foreach (var transactionDetails in transactionService.GetWalletTransactions())
            {
                Transactions.Add(new TransactionSummary
                {
                    Id = transactionDetails.Transaction.Hash.ToString(),
                    Type = transactionDetails.Transaction.GetLocalizableTransactionType(),
                    Time = transactionDetails.Time,

                    BlockIndex = transactionDetails.BlockIndex,

                    // TODO Handle unconfirmed transactions
                    Confirmations = (blockchainHeight - transactionDetails.BlockIndex + 1).ToString()
                });
            }
        }

        private void CopySelectedTransactionId()
        {
            if (!TransactionIsSelected)
            {
                return;
            }

            clipboardService.SetText(SelectedTransaction.Id);
        }
    }
}
