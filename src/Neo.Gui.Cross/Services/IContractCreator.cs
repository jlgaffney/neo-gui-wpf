using System;
using System.Collections.Generic;
using Neo.Cryptography.ECC;
using Neo.SmartContract;

namespace Neo.Gui.Cross.Services
{
    public interface IContractCreator
    {
        Contract CreateContract(byte[] script, IReadOnlyList<ContractParameterType> parameters);

        Contract GetLockAccountContract(ECPoint publicKey, DateTime unlockDate);

        Contract GetMultiSignatureContract(int minimumSignatures, IEnumerable<ECPoint> publicKeys);
    }
}
