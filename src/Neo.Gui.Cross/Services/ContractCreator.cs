using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neo.Cryptography.ECC;
using Neo.SmartContract;
using Neo.VM;

namespace Neo.Gui.Cross.Services
{
    public class ContractCreator : IContractCreator
    {
        public Contract GetLockAccountContract(ECPoint publicKey, DateTime unlockDate)
        {
            uint timestamp = unlockDate.ToTimestamp();
            using (var builder = new ScriptBuilder())
            {
                builder.EmitPush(publicKey);
                builder.EmitPush(timestamp);
                // Lock 2.0 in mainnet tx:4e84015258880ced0387f34842b1d96f605b9cc78b308e1f0d876933c2c9134b
                builder.EmitAppCall(UInt160.Parse("d3cce84d0800172d09c88ccad61130611bd047a4"));
                return Contract.Create(new[] { ContractParameterType.Signature }, builder.ToArray());
            }
        }

        public Contract GetMultiSignatureContract(int minimumSignatures, IEnumerable<ECPoint> publicKeys)
        {
            return Contract.CreateMultiSigContract(minimumSignatures, publicKeys.ToArray());
        }
    }
}
