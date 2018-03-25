using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Core.Data;
using Neo.UI.Core.Transactions.Parameters;
using Neo.UI.Core.Wallet.Initialization;
using Neo.Wallets;

namespace Neo.UI.Core.Wallet
{
    public interface IWalletController : IDisposable
    {
        void Initialize(IWalletInitializationParameters parameters);

        void Refresh();

        bool WalletIsOpen { get; }

        bool WalletCanBeMigrated(string walletPath);

        /// <summary>
        /// Migrates to newer wallet format. This method does not open the migrated wallet.
        /// </summary>
        /// <returns>File path of new migrated wallet</returns>
        string MigrateWallet(string walletPath, string password);

        void CreateWallet(string walletPath, string password, bool createWithAccount = true);

        void OpenWallet(string walletPath, string password);

        void CloseWallet();

        void CreateNewAccount();

        void ImportPrivateKeys(IEnumerable<string> wifPrivateKeys);

        void ImportCertificate(X509Certificate2 certificate);

        void ImportWatchOnlyAddress(string[] addressesToWatch);

        void AddLockContractAccount(string publicKey, uint unlockDateTime);

        void AddMultiSignatureContract(int minimunSignatureNumber, IEnumerable<string> publicKeys);

        void AddContractWithParameters(string reedemScript, string parameterList);

        bool DeleteAccount(string accountScriptHash);

        Task<InvokeResult> InvokeScript(byte[] script);

        decimal GetTransactionFee<TParameters>(TParameters transactionParameters) where TParameters : TransactionParameters;

        Task BuildSignAndRelayTransaction<TParameters>(TParameters transactionParameters) where TParameters : TransactionParameters;

        bool Sign(ContractParametersContext context);

        Task<bool> Relay(IInventory inventory);

        void SetNEP5WatchScriptHashes(IEnumerable<string> nep5ScriptHashes);

        IEnumerable<UInt160> GetNEP5WatchScriptHashes();

        /// <summary>
        /// Get all accounts addresses in the wallet.
        /// </summary>
        IEnumerable<string> GetAccountsAddresses();

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

        Task<IEnumerable<AssetDto>> GetWalletAssets();

        UInt160 GetChangeAddress();

        AccountContract GetAccountContract(string accountScriptHash);

        AccountKeyInfo GetAccountKeys(string accountScriptHash);

        Task<Transaction> GetTransaction(UInt256 hash);

        /// <summary>
        /// Gets the public keys that the specified script hash have voted for.
        /// </summary>
        /// <param name="voterScriptHash">Script hash of the account that voted</param>
        /// <returns>Enumerable collection of public keys</returns>
        Task<IEnumerable<string>> GetVotes(string voterScriptHash);

        Task<ContractStateDto> GetContractState(string scriptHash);

        Task<AssetStateDto> GetAssetState(string assetId);

        bool CanViewCertificate(string assetId);

        string GetAssetCertificateFilePath(string assetId);

        decimal CalculateBonus();
        
        decimal CalculateUnavailableBonusGas(uint height);

        bool WalletContainsAccount(string scriptHash);

        /// <summary>
        /// First class assets
        /// </summary>
        string GetFirstClassTokenAvailability(string assetId);

        /// <summary>
        /// NEP-5 assets
        /// </summary>
        string GetNEP5TokenAvailability(string scriptHash);

        Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));
        
        string ScriptToScriptHash(byte[] data);

        UInt160 AddressToScriptHash(string address);

        string ScriptHashToAddress(string scriptHash);

        bool AddressIsValid(string address);

        Task DeleteFirstClassAsset(string assetId);

        Task ClaimUtilityTokenAsset();

        IEnumerable<string> GetPublicKeysFromStandardAccounts();

        IEnumerable<string> GetAddressesForNonWatchOnlyAccounts();
    }
}