using System;
using System.Collections.Generic;
using Neo.Core;
using Neo.Cryptography.ECC;
using Neo.Gui.Base.Data;
using Neo.Network;
using Neo.SmartContract;
using Neo.Wallets;

namespace Neo.Gui.Base.Controllers
{
    public interface IWalletController : IDisposable
    {
        void Initialize(string certificateCachePath);

        bool WalletIsOpen { get; }

        uint WalletHeight { get; }

        bool WalletIsSynchronized { get; }

        bool WalletNeedUpgrade(string walletPath);

        void UpgradeWallet(string walletPath);

        void CreateWallet(string walletPath, string password);

        void OpenWallet(string walletPath, string password, bool repairMode);

        void CloseWallet();

        bool ChangePassword(string oldPassword, string newPassword);

        void RebuildCurrentWallet();

        void CreateNewKey();

        bool Sign(ContractParametersContext context);

        void Relay(Transaction transaction, bool saveTransaction = true);

        void Relay(IInventory inventory);

        void SetNEP5WatchScriptHashes(IEnumerable<string> nep5WatchScriptHashesHex);

        IEnumerable<UInt160> GetNEP5WatchScriptHashes();

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

        Transaction GetTransaction(UInt256 hash);

        Transaction GetTransaction(UInt256 hash, out int height);

        AccountState GetAccountState(UInt160 scriptHash);

        ContractState GetContractState(UInt160 scriptHash);

        AssetState GetAssetState(UInt256 assetId);

        bool CanViewCertificate(AssetItem item);

        Fixed8 CalculateBonus();

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd);
        
        Fixed8 CalculateUnavailableBonusGas(uint height);

        bool WalletContainsAddress(UInt160 scriptHash);

        BigDecimal GetAvailable(UIntBase assetId);

        Fixed8 GetAvailable(UInt256 assetId);

        void ImportWatchOnlyAddress(string addressToImport);

        void DeleteAccount(AccountItem account);

        Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        ContractTransaction MakeTransaction(ContractTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        InvocationTransaction MakeTransaction(InvocationTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        IssueTransaction MakeTransaction(IssueTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8));

        Transaction MakeClaimTransaction(CoinReference[] claims);

        UInt160 ToScriptHash(string address);

        string ToAddress(UInt160 scriptHash);
    }
}