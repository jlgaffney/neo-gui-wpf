using System;
using System.Collections.Generic;

using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Network;
using Neo.SmartContract;
using Neo.Wallets;

using Neo.Gui.Base.Data;

namespace Neo.Gui.Base.Controllers
{
    public interface IWalletController : IDisposable
    {
        void Initialize();

        bool WalletIsOpen { get; }

        uint WalletHeight { get; }

        bool WalletIsSynchronized { get; }

        bool WalletCanBeMigrated(string walletPath);

        /// <summary>
        /// Migrates to newer wallet format. This method does not open the migrated wallet.
        /// </summary>
        /// <returns>File path of new migrated wallet</returns>
        string MigrateWallet(string walletPath, string password, string newWalletPath = null);

        void CreateWallet(string walletPath, string password, bool createWithAccount = true);

        void OpenWallet(string walletPath, string password);

        void CloseWallet();

        bool ChangePassword(string oldPassword, string newPassword);

        void CreateNewAccount();

        bool Sign(ContractParametersContext context);

        void Relay(Transaction transaction, bool saveTransaction = true);

        void Relay(IInventory inventory);

        void SetNEP5WatchScriptHashes(IEnumerable<string> nep5WatchScriptHashesHex);

        IEnumerable<UInt160> GetNEP5WatchScriptHashes();

        /// <summary>
        /// Gets all accounts in wallets.
        /// </summary>
        IEnumerable<WalletAccount> GetAccounts();

        /// <summary>
        /// Gets accounts that are not watch-only (i.e. standard and non-standard contract accounts).
        /// </summary>
        IEnumerable<WalletAccount> GetNonWatchOnlyAccounts();

        /// <summary>
        /// Gets standard contract accounts.
        /// </summary>
        IEnumerable<WalletAccount> GetStandardAccounts();

        IEnumerable<Coin> GetCoins();

        IEnumerable<Coin> GetUnclaimedCoins();

        IEnumerable<Coin> FindUnspentCoins();

        UInt160 GetChangeAddress();

        Contract GetAccountContract(UInt160 accountScriptHash);

        KeyPair GetAccountKey(UInt160 accountScriptHash);

        Transaction GetTransaction(UInt256 hash);

        Transaction GetTransaction(UInt256 hash, out int height);

        AccountState GetAccountState(UInt160 scriptHash);

        ContractState GetContractState(UInt160 scriptHash);

        AssetState GetAssetState(UInt256 assetId);

        bool CanViewCertificate(FirstClassAssetItem assetItem);

        string ViewCertificate(FirstClassAssetItem assetItem);

        Fixed8 CalculateBonus();

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd);
        
        Fixed8 CalculateUnavailableBonusGas(uint height);

        bool WalletContainsAccount(UInt160 scriptHash);

        BigDecimal GetAvailable(UIntBase assetId);

        Fixed8 GetAvailable(UInt256 assetId);

        void ImportWatchOnlyAddress(string addressToImport);

        bool DeleteAccount(AccountItem account);

        Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        ContractTransaction MakeTransaction(ContractTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        InvocationTransaction MakeTransaction(InvocationTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        IssueTransaction MakeTransaction(IssueTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        Transaction MakeClaimTransaction(CoinReference[] claims);

        InvocationTransaction MakeValidatorRegistrationTransaction(ECPoint publicKey);

        InvocationTransaction MakeAssetCreationTransaction(AssetType? assetType, string assetName,
            Fixed8 amount, byte precision, ECPoint assetOwner, UInt160 assetAdmin, UInt160 assetIssuer);

        InvocationTransaction MakeContractCreationTransaction(byte[] script, byte[] parameterList, ContractParameterType returnType,
            bool needsStorage, string name, string version, string author, string email, string description);

        UInt160 ToScriptHash(string address);

        string ToAddress(UInt160 scriptHash);

        void DeleteFirstClassAsset(FirstClassAssetItem assetItem);
    }
}