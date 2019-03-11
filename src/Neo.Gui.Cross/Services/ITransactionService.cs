using System.Collections.Generic;
using Neo.Cryptography.ECC;
using Neo.Gui.Cross.Models;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using AssetType = Neo.Network.P2P.Payloads.AssetType;

namespace Neo.Gui.Cross.Services
{
    public interface ITransactionService
    {
        IEnumerable<TransactionStateDetails> GetWalletTransactions();



        ClaimTransaction CreateClaimTransaction();

        InvocationTransaction CreateContractCreationTransaction(byte[] script, byte[] parameterList, ContractParameterType returnType, ContractPropertyState properties, string name, string version, string author, string email, string description);

        InvocationTransaction CreateAssetRegistrationTransaction(AssetType assetType, string name, Fixed8 amount, byte precision, ECPoint owner, UInt160 admin, UInt160 issuer);

        StateTransaction CreateElectionTransaction(ECPoint publicKey);

        StateTransaction CreateVotingTransaction(UInt160 accountScriptHash, IEnumerable<ECPoint> votes);

    }
}
