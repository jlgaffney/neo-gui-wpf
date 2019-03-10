using System.Collections.Generic;
using System.Linq;
using Neo.Gui.Cross.Exceptions;
using Neo.Gui.Cross.Models;
using Neo.Ledger;
using Neo.Persistence;

namespace Neo.Gui.Cross.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IBlockchainService blockchainService;
        private readonly IWalletService walletService;

        public TransactionService(
            IBlockchainService blockchainService,
            IWalletService walletService)
        {
            this.blockchainService = blockchainService;
            this.walletService = walletService;
        }

        public IEnumerable<TransactionStateDetails> GetWalletTransactions()
        {
            ThrowIfWalletNotOpen();
            
            using (var snapshot = blockchainService.GetSnapshot())
            {
                foreach (var transactionInfo in walletService.CurrentWallet.GetTransactions()
                    .Select(transactionId => snapshot.Transactions.TryGet(transactionId))
                    .Where(transactionState => transactionState.Transaction != null)
                    .Select(transactionState => new TransactionStateDetails
                    {
                        Transaction = transactionState.Transaction,
                        BlockIndex = transactionState.BlockIndex,
                        Time = snapshot.GetHeader(transactionState.BlockIndex).Timestamp.ToDateTime()
                    })
                    .OrderBy(p => p.Time))
                {
                    yield return transactionInfo;
                }
            }
        }






        private void ThrowIfWalletNotOpen()
        {
            if (!walletService.WalletIsOpen)
            {
                throw new WalletNotOpenException();
            }
        }
    }
}
