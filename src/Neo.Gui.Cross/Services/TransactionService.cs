using System.Collections.Generic;
using System.Linq;
using Neo.Cryptography.ECC;
using Neo.Gui.Cross.Exceptions;
using Neo.Gui.Cross.Models;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
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

        public ClaimTransaction CreateClaimTransaction()
        {
            ThrowIfWalletNotOpen();

            var claims = walletService.CurrentWallet.GetUnclaimedCoins().Select(p => p.Reference).ToArray();
            if (claims.Length == 0)
            {
                // TODO Throw exception instead
                return null;
            }

            using (var snapshot = blockchainService.GetSnapshot())
            {
                return new ClaimTransaction
                {
                    Claims = claims,
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = new[]
                    {
                        new TransactionOutput
                        {
                            AssetId = Blockchain.UtilityToken.Hash,
                            Value = snapshot.CalculateBonus(claims),
                            ScriptHash = walletService.CurrentWallet.GetChangeAddress()
                        }
                    }
                };
            }
        }

        public StateTransaction CreateVotingTransaction(UInt160 accountScriptHash, IEnumerable<ECPoint> votes)
        {
            ThrowIfWalletNotOpen();

            return walletService.CurrentWallet.MakeTransaction(new StateTransaction
            {
                Version = 0,
                Descriptors = new[]
                {
                    new StateDescriptor
                    {
                        Type = StateType.Account,
                        Key = accountScriptHash.ToArray(),
                        Field = "Votes",
                        Value = votes.ToArray().ToByteArray()
                    }
                }
            });
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
