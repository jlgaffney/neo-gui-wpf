using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Neo.SmartContract;
using Neo.Wallets;

namespace Neo.Gui.Cross.Services
{
    public interface IAccountService
    {
        WalletAccount GetAccount(UInt160 scriptHash);

        IEnumerable<WalletAccount> GetAllAccounts();

        IEnumerable<WalletAccount> GetStandardAccounts();

        IEnumerable<WalletAccount> GetNonWatchOnlyAccounts();


        WalletAccount CreateStandardAccount();

        WalletAccount CreateContractAccount(byte[] script, IReadOnlyList<ContractParameterType> parameters);

        WalletAccount CreateLockContractAccount(string publicKey, DateTime unlockTime);

        WalletAccount CreateMultiSignatureContractAccount(int minimumSignatures, IEnumerable<string> publicKeys);

        WalletAccount ImportPrivateKey(string wif);

        WalletAccount ImportContract(Contract contract, byte[] privateKey);

        WalletAccount ImportCertificate(X509Certificate2 certificate);


        bool DeleteAccount(UInt160 scriptHash);
    }
}
