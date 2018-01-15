using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Core.Data;
using Neo.Wallets;

namespace Neo.UI.Core.Controllers.Interfaces
{
    public interface IWalletController : IDisposable
    {
        Fixed8 NetworkFee { get; }

        void Initialize();

        bool WalletIsOpen { get; }

        uint WalletHeight { get; }

        bool WalletIsSynchronized { get; }

        bool WalletCanBeMigrated(string walletPath);

        /// <summary>
        /// Migrates to newer wallet format. This method does not open the migrated wallet.
        /// </summary>
        /// <returns>File path of new migrated wallet</returns>
        string MigrateWallet(string walletPath, string password);

        void CreateWallet(string walletPath, string password, bool createWithAccount = true);

        void OpenWallet(string walletPath, string password);

        void CloseWallet();

        void CreateAccount(Contract contract = null);

        void ImportPrivateKeys(IEnumerable<string> wifPrivateKeys);

        void ImportCertificate(X509Certificate2 certificate);

        bool Sign(ContractParametersContext context);

        void Relay(Transaction transaction, bool saveTransaction = true);

        void Relay(IInventory inventory);
        
        void SignAndRelay(Transaction transaction);

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

        IEnumerable<Coin> FindUnspentCoins();

        UInt160 GetChangeAddress();

        AccountContract GetAccountContract(string accountScriptHash);

        KeyPair GetAccountKey(UInt160 accountScriptHash);

        Transaction GetTransaction(UInt256 hash);

        Transaction GetTransaction(UInt256 hash, out int height);

        /// <summary>
        /// Gets the public keys that the specified script hash have voted for.
        /// </summary>
        /// <param name="scriptHash">Script hash of the account that voted</param>
        /// <returns>Enumerable collection of public keys</returns>
        IEnumerable<ECPoint> GetVotes(UInt160 scriptHash);

        ContractState GetContractState(UInt160 scriptHash);

        AssetState GetAssetState(UInt256 assetId);

        bool CanViewCertificate(FirstClassAssetItem assetItem);

        string ViewCertificate(FirstClassAssetItem assetItem);

        Fixed8 CalculateBonus();

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd);
        
        Fixed8 CalculateUnavailableBonusGas(uint height);

        bool WalletContainsAccount(UInt160 scriptHash);

        /// <summary>
        /// NEP-5 assets
        /// </summary>
        BigDecimal GetAvailable(UInt160 assetId);

        /// <summary>
        /// First class assets
        /// </summary>
        Fixed8 GetAvailable(UInt256 assetId);

        void ImportWatchOnlyAddress(string[] addressesToWatch);

        bool DeleteAccount(AccountItem account);

        Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        InvocationTransaction MakeValidatorRegistrationTransaction(ECPoint publicKey);

        InvocationTransaction MakeAssetCreationTransaction(
            AssetType? assetType, 
            string assetName,
            Fixed8 amount, 
            byte precision, 
            ECPoint assetOwner, 
            UInt160 assetAdmin, 
            UInt160 assetIssuer);

        InvocationTransaction MakeContractCreationTransaction(
            byte[] script, 
            byte[] parameterList, 
            ContractParameterType returnType,
            bool needsStorage, 
            string name, 
            string version, 
            string author, 
            string email, 
            string description);

        UInt160 AddressToScriptHash(string address);

        string ScriptHashToAddress(UInt160 scriptHash);

        bool AddressIsValid(string address);

        void DeleteFirstClassAsset(FirstClassAssetItem assetItem);

        void ClaimUtilityTokenAsset();

        void IssueAsset(UInt256 assetId, IEnumerable<TransferOutput> items);

        void InvokeContract(InvocationTransaction transaction);

        Transaction MakeTransferTransaction(
            IEnumerable<TransferOutput> items, 
            string remark, 
            UInt160 changeAddress = null, 
            Fixed8 fee = default(Fixed8));

        void AddLockContractAccount(string publicKey, uint unlockDateTime);

        IEnumerable<string> GetPublicKeysFromStandardAccounts();

        void AddMultiSignatureContract(int minimunSignatureNumber, IEnumerable<string> publicKeys);

        void AddContractWithParameters(string reedemScript, string parameterList);
    }
}