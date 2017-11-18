using System.Collections.Generic;
using Neo.Core;
using Neo.SmartContract;
using Neo.UI;
using Neo.Wallets;

namespace Neo.Controllers
{
    public interface IWalletController
    {
        bool WalletIsOpen { get; }

        uint WalletHeight { get; }

        void CreateWallet(string walletPath, string password);

        void OpenWallet(string walletPath, string password, bool repairMode);

        void CloseWallet();

        bool ChangePassword(string oldPassword, string newPassword);

        void RebuildWalletIndexes();

        void SaveTransaction(Transaction transaction);

        void Sign(ContractParametersContext context);

        void CreateNewKey();        // TODO - Issue #43 [AboimPinto] - this method will create a new key or new NEO address? Please review the name.

        IEnumerable<KeyPair> GetKeys();

        IEnumerable<UInt160> GetAddresses();

        IEnumerable<VerificationContract> GetContracts(UInt160 publicKeyHash);

        IEnumerable<Coin> GetCoins();

        VerificationContract GetContract(UInt160 scriptHash);

        KeyPair GetKeyByScriptHash(UInt160 scriptHash);

        void ImportWatchOnlyAddress(string addressToImport);

        void DeleteAccount(AccountItem account);
    }
}
