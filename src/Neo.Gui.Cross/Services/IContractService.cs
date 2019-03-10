using System;
using System.Collections.Generic;
using Neo.Cryptography.ECC;
using Neo.SmartContract;

namespace Neo.Gui.Cross.Services
{
    public interface IContractService
    {
        Contract CreateContract(byte[] script, IReadOnlyList<ContractParameterType> parameters);

        Contract CreateLockAccountContract(ECPoint publicKey, DateTime unlockDate);

        Contract CreateMultiSignatureContract(int minimumSignatures, IEnumerable<ECPoint> publicKeys);
    }
}
