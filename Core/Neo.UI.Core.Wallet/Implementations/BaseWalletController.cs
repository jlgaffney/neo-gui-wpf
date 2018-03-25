using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Implementations.Wallets.NEP6;
using Neo.IO.Json;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Core.Data;
using Neo.UI.Core.Data.Enums;
using Neo.UI.Core.Globalization.Resources;
using Neo.UI.Core.Helpers;
using Neo.UI.Core.Helpers.Extensions;
using Neo.UI.Core.Internal.Services.Interfaces;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Services.Interfaces;
using Neo.UI.Core.Transactions.Interfaces;
using Neo.UI.Core.Transactions.Parameters;
using Neo.UI.Core.Wallet.Data;
using Neo.UI.Core.Wallet.Exceptions;
using Neo.UI.Core.Wallet.Helpers;
using Neo.UI.Core.Wallet.Initialization;
using Neo.UI.Core.Wallet.Messages;
using Neo.VM;
using Neo.Wallets;
using BaseWallet = Neo.Wallets.Wallet;
using DeprecatedWallet = Neo.Implementations.Wallets.EntityFramework.UserWallet;
using ECPoint = Neo.Cryptography.ECC.ECPoint;

namespace Neo.UI.Core.Wallet.Implementations
{
    internal abstract class BaseWalletController
    {
        protected const string GoverningTokenAssetId = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        protected const string UtilityTokenAssetId = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        protected static readonly Fixed8 NetworkFee = Fixed8.FromDecimal(0.001m);

        private readonly ICertificateQueryService certificateQueryService;
        private readonly IMessagePublisher messagePublisher;
        private readonly INotificationService notificationService;
        private readonly ITransactionBuilderFactory transactionBuilderFactory;

        private readonly Dictionary<ECPoint, CertificateQueryResult> certificateQueryResultCache;

        protected BaseWallet currentWallet;
        protected IDisposable currentWalletLocker;
        protected WalletInfo currentWalletInfo;

        protected AssetInfoCache assetInfoCache;

        protected UInt160[] nep5WatchScriptHashes;

        internal BaseWalletController(
            ICertificateQueryService certificateQueryService,
            IMessagePublisher messagePublisher,
            INotificationService notificationService,
            ITransactionBuilderFactory transactionBuilderFactory)
        {
            this.certificateQueryService = certificateQueryService;
            this.messagePublisher = messagePublisher;
            this.notificationService = notificationService;
            this.transactionBuilderFactory = transactionBuilderFactory;

            this.certificateQueryResultCache = new Dictionary<ECPoint, CertificateQueryResult>();

            this.assetInfoCache = new AssetInfoCache();
        }
        
        public bool WalletIsOpen => this.currentWallet != null;

        public void Refresh()
        {
            Task.Run(() => this.RefreshWallet());
        }

        protected void Initialize(BaseWalletInitializationParameters parameters)
        {
            this.certificateQueryService.Initialize(parameters.CertificateCachePath);
        }

        protected virtual void SetCurrentWallet(BaseWallet wallet, IDisposable walletLocker)
        {
            if (this.WalletIsOpen)
            {
                // Try save wallet in case something was not saved
                this.TrySaveWallet();

                // Dispose of wallet if required
                this.WalletDispose();
            }

            this.currentWallet = wallet;
            this.currentWalletLocker = walletLocker;
            this.currentWalletInfo = new WalletInfo();

            this.messagePublisher.Publish(new CurrentWalletHasChangedMessage());
        }

        protected abstract void RefreshWallet();

        public abstract Task<bool> Relay(IInventory inventory);

        public decimal GetTransactionFee<TParameters>(TParameters transactionParameters)
            where TParameters : TransactionParameters
        {
            var transaction = this.BuildTransaction(transactionParameters);

            return (decimal) transaction.SystemFee;
        }

        protected async Task SignAndRelay(Transaction transaction)
        {
            Guard.ArgumentIsNotNull(transaction, nameof(transaction));

            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(transaction);
            }
            catch (InvalidOperationException)
            {
                this.notificationService.ShowErrorNotification(Strings.UnsynchronizedBlock);
                return;
            }

            this.Sign(context);

            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();

                var success = await this.Relay(transaction);

                if (success)
                {
                    this.notificationService.ShowSuccessNotification($"{Strings.SendTxSucceedMessage} {transaction.Hash}");
                }
                else
                {
                    this.notificationService.ShowErrorNotification("Transaction was not sent successfully!");
                }
            }
            else
            {
                this.notificationService.ShowErrorNotification($"{Strings.IncompletedSignatureMessage} {context}");
            }
        }

        protected virtual Transaction BuildTransaction<TParameters>(TParameters parameters) where TParameters : TransactionParameters
        {
            var builder = this.transactionBuilderFactory.GetBuilder(parameters);

            return builder.Build(parameters);
        }

        protected void TrySaveWallet()
        {
            var nep6Wallet = this.currentWallet as NEP6Wallet;
            nep6Wallet?.Save();
        }

        protected void WalletDispose()
        {
            this.currentWalletLocker?.Dispose();
            this.currentWalletLocker = null;

            var disposableWallet = this.currentWallet as IDisposable;
            disposableWallet?.Dispose();
            this.currentWallet = null;
        }
        
        public bool WalletCanBeMigrated(string walletPath)
        {
            return Path.GetExtension(walletPath) == ".db3";
        }

        public string MigrateWallet(string walletPath, string password)
        {
            var newWalletPath = Path.ChangeExtension(walletPath, ".json");
            newWalletPath = PathHelper.GetAvailableFilePath(newWalletPath);

            NEP6Wallet nep6Wallet;
            try
            {
                nep6Wallet = NEP6Wallet.Migrate(newWalletPath, walletPath, password);
            }
            catch (CryptographicException)
            {
                this.notificationService.ShowErrorNotification(Strings.PasswordIncorrect);
                return null;
            }

            // Migration successful
            nep6Wallet.Save();
            nep6Wallet.Dispose();

            this.notificationService.ShowInformationNotification(Strings.MigrateWalletSucceedMessage + newWalletPath);

            return newWalletPath;
        }

        public void CreateWallet(string walletPath, string password, bool createWithAccount = true)
        {
            var newWallet = new NEP6Wallet(walletPath);

            var walletLocker = newWallet.Unlock(password);

            this.SetCurrentWallet(newWallet, walletLocker);

            if (createWithAccount)
            {
                this.CreateNewAccount();
            }
        }

        public void OpenWallet(string walletPath, string password)
        {
            BaseWallet wallet;
            IDisposable walletLocker;
            if (Path.GetExtension(walletPath) == ".db3")
            {
                DeprecatedWallet userWallet;
                try
                {
                    userWallet = DeprecatedWallet.Open(walletPath, password);
                    walletLocker = null;
                }
                catch (CryptographicException)
                {
                    this.notificationService.ShowErrorNotification(Strings.PasswordIncorrect);
                    return;
                }
                wallet = userWallet;
            }
            else
            {
                var nep6Wallet = new NEP6Wallet(walletPath);
                try
                {
                    walletLocker = nep6Wallet.Unlock(password);
                }
                catch (CryptographicException)
                {
                    this.notificationService.ShowErrorNotification(Strings.PasswordIncorrect);
                    return;
                }
                wallet = nep6Wallet;
            }

            if (wallet == null)
            {
                // TODO Localise text
                this.notificationService.ShowErrorNotification("Could not open wallet! An error occurred while opening");
                return;
            }

            this.SetCurrentWallet(wallet, walletLocker);
        }

        public void CloseWallet()
        {
            this.SetCurrentWallet(null, null);
        }

        public async Task BuildSignAndRelayTransaction<TParameters>(TParameters transactionParameters) where TParameters : TransactionParameters
        {
            Guard.ArgumentIsNotNull(transactionParameters, nameof(transactionParameters));

            var transaction = this.BuildTransaction(transactionParameters);

            await this.SignAndRelay(transaction);
        }

        public bool Sign(ContractParametersContext context)
        {
            Guard.ArgumentIsNotNull(context, nameof(context));

            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.Sign(context);
        }

        public bool WalletContainsAccount(string scriptHashStr)
        {
            this.ThrowIfWalletIsNotOpen();

            var scriptHash = UInt160.Parse(scriptHashStr);

            return this.currentWallet.Contains(scriptHash);
        }

        public IEnumerable<WalletAccount> GetAccounts()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.GetAccounts();
        }

        public IEnumerable<string> GetAccountsAddresses()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts()
                .Select(x => x.Address)
                .ToList();
        }

        public IEnumerable<WalletAccount> GetNonWatchOnlyAccounts()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts().Where(account => !account.WatchOnly);
        }

        public IEnumerable<WalletAccount> GetStandardAccounts()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts()
                .Where(account => !account.WatchOnly && account.Contract.IsStandard);
        }

        public IEnumerable<string> GetPublicKeysFromStandardAccounts()
        {
            return this
                .GetStandardAccounts()
                .Select(x => x.GetKey().PublicKey.ToString())
                .ToList();
        }

        public IEnumerable<string> GetAddressesForNonWatchOnlyAccounts()
        {
            return this
                .GetNonWatchOnlyAccounts()
                .Select(x => x.Address.ToString())
                .ToList();
        }

        public UInt160 GetChangeAddress()
        {
            this.ThrowIfWalletIsNotOpen();

            return this.currentWallet.GetChangeAddress();
        }


        public void CreateNewAccount()
        {
            this.CreateAccount(null);
        }

        public virtual bool DeleteAccount(string accountScriptHash)
        {
            this.ThrowIfWalletIsNotOpen();

            Guard.ArgumentIsNotNull(accountScriptHash, nameof(accountScriptHash));

            var deletedSuccessfully = this.currentWallet.DeleteAccount(UInt160.Parse(accountScriptHash));

            if (!deletedSuccessfully) return false;

            this.TrySaveWallet();

            return true;
        }

        public void ImportPrivateKeys(IEnumerable<string> wifPrivateKeys)
        {
            if (wifPrivateKeys == null) return;

            var wifList = wifPrivateKeys.ToList();

            if (!wifList.Any()) return;

            foreach (var wif in wifList)
            {
                WalletAccount account;
                try
                {
                    account = this.currentWallet.Import(wif);
                }
                catch (FormatException)
                {
                    // Skip WIF
                    continue;
                }

                this.AddAccountItem(account);
            }

            this.TrySaveWallet();
        }

        public void ImportCertificate(X509Certificate2 certificate)
        {
            if (certificate == null) return;

            WalletAccount account;
            try
            {
                account = this.currentWallet.Import(certificate);
            }
            catch
            {
                // TODO Localise this text
                this.notificationService.ShowErrorNotification("Certificate import failed!");
                return;
            }

            this.AddAccountItem(account);

            this.TrySaveWallet();
        }

        public void AddLockContractAccount(string publicKey, uint unlockDateTime)
        {
            using (var sb = new ScriptBuilder())
            {
                sb.EmitPush(publicKey.ToECPoint());
                sb.EmitPush(unlockDateTime);
                // Lock 2.0 in mainnet tx:4e84015258880ced0387f34842b1d96f605b9cc78b308e1f0d876933c2c9134b
                sb.EmitAppCall(UInt160.Parse("d3cce84d0800172d09c88ccad61130611bd047a4"));

                try
                {
                    var contract = Contract.Create(new[] { ContractParameterType.Signature }, sb.ToArray());
                    this.CreateAccount(contract);
                }
                catch
                {
                    this.notificationService.ShowErrorNotification(Strings.AddContractFailedMessage);
                }
            }
        }

        public void AddMultiSignatureContract(int minimunSignatureNumber, IEnumerable<string> publicKeys)
        {
            var ecPoints = publicKeys
                .Select(p => p.ToECPoint())
                .ToArray();

            var contract = Contract.CreateMultiSigContract(minimunSignatureNumber, ecPoints);

            if (contract == null) return;
            this.CreateAccount(contract);
        }

        public void AddContractWithParameters(string reedemScript, string parameterList)
        {
            var parameters = parameterList.HexToBytes().Select(p => (ContractParameterType)p).ToArray();
            var scriptBytes = reedemScript.HexToBytes();

            var contract = Contract.Create(parameters, scriptBytes);

            this.CreateAccount(contract);
        }

        public void ImportWatchOnlyAddress(string[] addressesToWatch)
        {
            foreach (var address in addressesToWatch)
            {
                if (address == null) continue;

                var trimmedAddress = address.Trim();

                if (string.IsNullOrEmpty(trimmedAddress)) continue;

                UInt160 scriptHash;
                try
                {
                    scriptHash = this.AddressToScriptHash(trimmedAddress);
                }
                catch (FormatException)
                {
                    continue;
                }

                var account = this.currentWallet.CreateAccount(scriptHash);

                this.AddAccountItem(account);
            }

            this.TrySaveWallet();
        }

        public AccountContract GetAccountContract(string accountScriptHash)
        {
            var walletAccount = this.GetWalletAccount(UInt160.Parse(accountScriptHash));

            if (walletAccount == null) return null;

            var accountContract = new AccountContract
            {
                Address = walletAccount.Contract.Address,
                ParameterList = walletAccount.Contract.ParameterList.Cast<byte>().ToArray().ToHexString(),
                ScriptHash = walletAccount.Contract.ScriptHash.ToString(),
                RedeemScriptHex = walletAccount.Contract.Script.ToHexString()

            };

            return accountContract;
        }

        public AccountKeyInfo GetAccountKeys(string accountScriptHash)
        {
            var walletAccount = this.GetWalletAccount(UInt160.Parse(accountScriptHash));

            if (walletAccount == null) return null;
            if (!walletAccount.HasKey) return null;

            var accountKeys = new AccountKeyInfo
            {
                Address = accountScriptHash,
                PublicKeyHex = walletAccount.GetKey().PublicKey.EncodePoint(true).ToHexString()
            };

            using (walletAccount.GetKey().Decrypt())
            {
                accountKeys.PrivateKeyHex = walletAccount.GetKey().PrivateKey.ToHexString();
            }

            return accountKeys;
        }

        protected WalletAccount GetWalletAccount(UInt160 scriptHash)
        {
            if (scriptHash == null) return null;

            this.ThrowIfWalletIsNotOpen();

            return this.GetAccounts()
                .FirstOrDefault(account => scriptHash.Equals(account.ScriptHash));
        }

        private void CreateAccount(Contract contract)
        {
            this.ThrowIfWalletIsNotOpen();

            WalletAccount account;
            if (contract == null)
            {
                account = this.currentWallet.CreateAccount();
            }
            else
            {
                account = this.currentWallet.CreateAccount(contract);
            }

            this.AddAccountItem(account);

            this.TrySaveWallet();
        }

        protected void AddAccountItem(WalletAccount account)
        {
            AccountType accountType;

            if (account.WatchOnly)
            {
                accountType = AccountType.WatchOnly;
            }
            else
            {
                accountType = account.Contract.IsStandard
                    ? AccountType.Standard
                    : AccountType.NonStandard;
            }

            this.messagePublisher.Publish(new AccountAddedMessage(account.Label, account.Address, account.ScriptHash.ToString(), accountType));
        }

        protected void AddTransaction(TransactionMetadata transaction)
        {
            var existingTransaction = this.currentWalletInfo.GetTransaction(transaction.Id);

            if (existingTransaction != null) return;

            this.currentWalletInfo.AddTransaction(transaction);
            this.messagePublisher.Publish(new TransactionAddedMessage(transaction.Id.ToString(), transaction.Time, transaction.Height, transaction.Type.ToString()));
        }

        public bool CanViewCertificate(string assetIdStr)
        {
            if (assetIdStr == GoverningTokenAssetId ||
                assetIdStr == UtilityTokenAssetId) return false;

            if (!UInt256.TryParse(assetIdStr, out var assetId)) return false;

            var asset = this.assetInfoCache.GetAssetInfo(assetId);

            if (asset?.AssetOwner == null) return false;

            var queryResult = this.GetCertificateQueryResult(asset.AssetOwner);

            if (queryResult == null) return false;

            return queryResult.Type == CertificateQueryResultType.Good ||
                   queryResult.Type == CertificateQueryResultType.Expired ||
                   queryResult.Type == CertificateQueryResultType.Invalid;
        }

        public string GetAssetCertificateFilePath(string assetIdStr)
        {
            if (!UInt256.TryParse(assetIdStr, out var assetId)) return null;

            var asset = this.assetInfoCache.GetAssetInfo(assetId);

            if (asset?.AssetOwner == null) return null;

            return this.certificateQueryService.GetCachedCertificatePath(asset.AssetOwner);
        }

        public string GetFirstClassTokenAvailability(string assetIdStr)
        {
            if (!UInt256.TryParse(assetIdStr, out var assetId)) return null;

            if (!this.WalletIsOpen) return null;

            var totalBalance = this.currentWalletInfo.GetAssetTotalBalance(assetId);

            return totalBalance.ToString();
        }

        public string GetNEP5TokenAvailability(string scriptHashStr)
        {
            if (!UInt160.TryParse(scriptHashStr, out var scriptHash)) return null;

            if (!this.WalletIsOpen) return null;

            var totalBalance = this.currentWalletInfo.GetNEP5TokenTotalBalance(scriptHash);

            return totalBalance.ToString();
        }


        public string ScriptToScriptHash(byte[] data)
        {
            return data.ToScriptHash().ToString();
        }

        public UInt160 AddressToScriptHash(string address)
        {
            return BaseWallet.ToScriptHash(address);
        }

        public string ScriptHashToAddress(string scriptHash)
        {
            return BaseWallet.ToAddress(UInt160.Parse(scriptHash));
        }

        public bool AddressIsValid(string address)
        {
            try
            {
                this.AddressToScriptHash(address);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public void SetNEP5WatchScriptHashes(IEnumerable<string> nep5ScriptHashes)
        {
            var scriptHashes = new List<UInt160>();

            foreach (var scriptHashHex in nep5ScriptHashes)
            {
                if (!UInt160.TryParse(scriptHashHex, out var scriptHash)) continue;

                scriptHashes.Add(scriptHash);
            }

            this.nep5WatchScriptHashes = scriptHashes.Distinct().ToArray();
        }

        public IEnumerable<UInt160> GetNEP5WatchScriptHashes()
        {
            return this.nep5WatchScriptHashes;
        }



        /// <summary>
        /// Throws <see cref="WalletIsNotOpenException" /> if a wallet is not open.
        /// </summary>
        protected void ThrowIfWalletIsNotOpen()
        {
            if (this.WalletIsOpen) return;

            throw new WalletIsNotOpenException();
        }

        protected static InvokeResult GetInvokeResult(VMState engineState, Fixed8 gasConsumed, IEnumerable<string> evaluationStackJson)
        {
            var invokeFee = Fixed8.Zero;
            var executionSucceeded = false;
            if (!engineState.HasFlag(VMState.FAULT))
            {
                executionSucceeded = true;

                invokeFee = gasConsumed - Fixed8.FromDecimal(10);

                if (invokeFee < Fixed8.Zero)
                {
                    invokeFee = Fixed8.Zero;
                }

                invokeFee = invokeFee.Ceiling();

                if (invokeFee.Equals(Fixed8.Zero))
                {
                    invokeFee = NetworkFee;
                }
            }

            var builder = new StringBuilder();

            builder.AppendLine($"VM State: {engineState}");
            builder.AppendLine($"Gas Consumed: {gasConsumed}");
            builder.AppendLine($"Evaluation Stack: {new JArray(evaluationStackJson.Select(JObject.Parse))}");

            var resultStr = builder.ToString();

            return new InvokeResult(executionSucceeded, resultStr, (decimal) invokeFee);
        }

        protected void CheckAssetIssuerCertificates()
        {
            foreach (var assetId in this.currentWalletInfo.GetAssetsInWallet())
            {
                var assetInfo = this.assetInfoCache.GetAssetInfo(assetId);

                if (assetInfo.IssuerCertificateChecked || assetInfo.AssetOwner == null) continue;

                var queryResult = this.GetCertificateQueryResult(assetInfo.AssetOwner);

                if (queryResult == null) continue;

                switch (queryResult.Type)
                {
                    case CertificateQueryResultType.System:
                    case CertificateQueryResultType.Missing:
                    case CertificateQueryResultType.Good:
                    case CertificateQueryResultType.Expired:
                    case CertificateQueryResultType.Invalid:
                        assetInfo.IssuerCertificateChecked = true;
                        break;
                }

                if (queryResult.Type == CertificateQueryResultType.Good ||
                    queryResult.Type == CertificateQueryResultType.Expired ||
                    queryResult.Type == CertificateQueryResultType.Invalid)
                {
                    assetInfo.OwnerCertificate = queryResult.Certificate;

                    string issuer = null;
                    switch (queryResult.Type)
                    {
                        case CertificateQueryResultType.Good:
                            issuer = $"{queryResult.Certificate.Subject}[{assetInfo.AssetOwner}]";
                            break;
                        case CertificateQueryResultType.Expired:
                            issuer = $"[{Strings.ExpiredCertificate}]{queryResult.Certificate.Subject}[{assetInfo.AssetOwner}]";
                            break;
                        case CertificateQueryResultType.Invalid:
                            issuer = $"[{Strings.InvalidCertificate}][{assetInfo.AssetOwner}]";
                            break;
                    }

                    if (issuer != null)
                    {
                        this.messagePublisher.Publish(new AssetIssuerInfoUpdatedMessage(assetId.ToString(), issuer));
                    }
                }
            }
        }

        private CertificateQueryResult GetCertificateQueryResult(ECPoint publicKey)
        {
            // Check if certificate has been cached from a previous query
            if (this.certificateQueryResultCache.ContainsKey(publicKey))
            {
                return this.certificateQueryResultCache[publicKey];
            }

            // Query for certificate
            var result = this.certificateQueryService.Query(publicKey);

            if (result == null) return null;

            // Cache certificate query result
            this.certificateQueryResultCache.Add(publicKey, result);

            return result;
        }
    }
}
