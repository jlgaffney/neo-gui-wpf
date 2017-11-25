using System.Collections.Generic;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Gui.Base.Data;
using Neo.SmartContract;
using Neo.Wallets;

namespace Neo.Gui.Base.Controllers.Interfaces
{
    public interface IWalletController
    {
        void Initialize();

        void Shutdown();

        bool WalletIsOpen { get; }

        uint WalletHeight { get; }

        bool WalletIsSynchronized { get; }

        bool WalletNeedUpgrade(string walletPath);

        void UpgradeWallet(string walletPath);

        void CreateWallet(string walletPath, string password);

        void OpenWallet(string walletPath, string password, bool repairMode);

        void CloseWallet();

        bool ChangePassword(string oldPassword, string newPassword);

        void RebuildWalletIndexes();

        void SaveTransaction(Transaction transaction);

        bool Sign(ContractParametersContext context);

        void CreateNewKey();

        KeyPair GetKeyByScriptHash(UInt160 scriptHash);

        KeyPair GetKey(ECPoint publicKey);

        KeyPair GetKey(UInt160 publicKeyHash);

        IEnumerable<KeyPair> GetKeys();

        IEnumerable<UInt160> GetAddresses();

        VerificationContract GetContract(UInt160 scriptHash);

        IEnumerable<VerificationContract> GetContracts();

        IEnumerable<VerificationContract> GetContracts(UInt160 publicKeyHash);

        IEnumerable<Coin> GetCoins();

        IEnumerable<Coin> GetUnclaimedCoins();

        IEnumerable<Coin> FindUnspentCoins();
        
        UInt160 GetChangeAddress();

        bool WalletContainsAddress(UInt160 scriptHash);

        BigDecimal GetAvailable(UIntBase assetId);

        Fixed8 GetAvailable(UInt256 assetId);

        void ImportWatchOnlyAddress(string addressToImport);

        void DeleteAccount(AccountItem account);

        Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        ContractTransaction MakeTransaction(ContractTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        InvocationTransaction MakeTransaction(InvocationTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        IssueTransaction MakeTransaction(IssueTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        bool CanViewCertificate(AssetItem item);
    }
}