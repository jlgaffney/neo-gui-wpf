using System;
using System.Collections.Generic;
using System.Linq;
using Neo.Cryptography.ECC;
using Neo.Ledger;
using Neo.SmartContract;
using Neo.VM;

namespace Neo.Gui.Cross.Services
{
    public class ContractService : IContractService
    {
        private readonly IBlockchainService blockchainService;

        public ContractService(
            IBlockchainService blockchainService)
        {
            this.blockchainService = blockchainService;
        }

        public ContractState GetContractState(UInt160 scriptHash)
        {
            using (var snapshot = blockchainService.GetSnapshot())
            {
                return snapshot.Contracts.TryGet(scriptHash);
            }
        }


        public Contract CreateContract(byte[] script, IReadOnlyList<ContractParameterType> parameters)
        {
            return Contract.Create(parameters.ToArray(), script);
        }

        public Contract CreateLockAccountContract(ECPoint publicKey, DateTime unlockDate)
        {
            var timestamp = unlockDate.ToTimestamp();
            using (var builder = new ScriptBuilder())
            {
                builder.EmitPush(publicKey);
                builder.EmitPush(timestamp);
                // Lock 2.0 in mainnet tx:4e84015258880ced0387f34842b1d96f605b9cc78b308e1f0d876933c2c9134b
                builder.EmitAppCall(UInt160.Parse("d3cce84d0800172d09c88ccad61130611bd047a4"));
                return Contract.Create(new[] { ContractParameterType.Signature }, builder.ToArray());
            }
        }

        public Contract CreateMultiSignatureContract(int minimumSignatures, IEnumerable<ECPoint> publicKeys)
        {
            return Contract.CreateMultiSigContract(minimumSignatures, publicKeys.ToArray());
        }



    }
}
