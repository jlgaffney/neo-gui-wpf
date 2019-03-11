using System.Collections.Generic;
using Neo.Cryptography.ECC;
using Neo.Gui.Cross.Models;
using Neo.Network.P2P.Payloads;

namespace Neo.Gui.Cross.Services
{
    public interface ITransactionService
    {
        IEnumerable<TransactionStateDetails> GetWalletTransactions();



        ClaimTransaction CreateClaimTransaction();

        StateTransaction CreateVotingTransaction(UInt160 accountScriptHash, IEnumerable<ECPoint> votes);

    }
}
