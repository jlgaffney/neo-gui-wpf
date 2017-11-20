using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Neo.Core;
using Neo.DialogResults;
using Neo.Helpers;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI;
using Neo.UI.Base.Messages;
using Neo.UI.Messages;
using Neo.Wallets;
using ECPoint = Neo.Cryptography.ECC.ECPoint;

namespace Neo.Controllers
{
    public class WalletController : 
        IWalletController,
        IMessageHandler<AddContractsMessage>, 
        IMessageHandler<AddContractMessage>, 
        IMessageHandler<ImportPrivateKeyMessage>, 
        IMessageHandler<ImportCertificateMessage>,
        IMessageHandler<SignTransactionAndShowInformationMessage>
    {
        private const string MinimumMigratedWalletVersion = "1.3.5";

        #region Private Fields 
        private readonly IBlockChainController blockChainController;
        private readonly IDialogHelper dialogHelper;
        private readonly INotificationHelper notificationHelper;
        private readonly IMessagePublisher messagePublisher;

        private UserWallet currentWallet;

        private bool balanceChanged;
        private bool checkNep5Balance;

        private readonly IList<AccountItem> accounts;
        #endregion

        #region Constructor 
        public WalletController(
            IBlockChainController blockChainController,
            IDialogHelper dialogHelper,
            INotificationHelper notificationHelper,
            IMessagePublisher messagePublisher,
            IMessageSubscriber messageSubscriber)
        {
            this.blockChainController = blockChainController;
            this.dialogHelper = dialogHelper;
            this.notificationHelper = notificationHelper;
            this.messagePublisher = messagePublisher;

            messageSubscriber.Subscribe(this);

            this.accounts = new List<AccountItem>();
        }
        #endregion

        #region IWalletController implementation 
        public bool WalletIsOpen => this.currentWallet != null;

        public uint WalletHeight => !this.WalletIsOpen ? 0 : this.currentWallet.WalletHeight;

        public void CreateWallet(string walletPath, string password)
        {
            var newWallet = UserWallet.Create(walletPath, password);

            this.SetCurrentWallet(newWallet);

            Settings.Default.LastWalletPath = walletPath;
            Settings.Default.Save();
        }

        public void OpenWallet(string walletPath, string password, bool repairMode)
        {
            if (UserWallet.GetVersion(walletPath) < Version.Parse(MinimumMigratedWalletVersion))
            {
                // TODO - Issue #44 - [AboimPinto] - DialogHelper is not implemented yet.
                var migrationApproved = this.dialogHelper.ShowDialog<YesOrNoDialogResult>("ApproveWalletMigrationDialog");

                if (!migrationApproved.Yes)
                {
                    return;
                }

                this.MigrateWallet(walletPath);
                //this.dialogHelper.ShowDialog("WalletMigrationCompleteDialog");
            }

            var userWallet = this.OpenWalletWithPath(walletPath, password);
            if (userWallet == null)
            {
                return;
            }

            if (repairMode)
            {
                userWallet.Rebuild();
            }
            this.SetCurrentWallet(userWallet);

            Settings.Default.LastWalletPath = walletPath;
            Settings.Default.Save();
        }

        public void CloseWallet()
        {
            this.SetCurrentWallet(null);
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if (!this.WalletIsOpen) return false;

            return this.currentWallet.ChangePassword(oldPassword, newPassword);
        }

        public void RebuildWalletIndexes()
        {
            this.currentWallet.Rebuild();
        }

        public void SaveTransaction(Transaction transaction)
        {
            this.currentWallet.SaveTransaction(transaction);
        }

        public bool Sign(ContractParametersContext context)
        {
            return this.currentWallet.Sign(context);
        }

        public KeyPair GetKeyByScriptHash(UInt160 scriptHash)
        {
            return this.currentWallet?.GetKeyByScriptHash(scriptHash);
        }

        public KeyPair GetKey(ECPoint publicKey)
        {
            return this.currentWallet?.GetKey(publicKey);
        }

        public KeyPair GetKey(UInt160 publicKeyHash)
        {
            return this.currentWallet?.GetKey(publicKeyHash);
        }

        public IEnumerable<KeyPair> GetKeys()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<KeyPair>();
            }

            return this.currentWallet.GetKeys();
        }

        public IEnumerable<UInt160> GetAddresses()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<UInt160>();
            }

            return this.currentWallet.GetAddresses();
        }

        public IEnumerable<VerificationContract> GetContracts()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<VerificationContract>();
            }

            return this.currentWallet.GetContracts();
        }

        public IEnumerable<VerificationContract> GetContracts(UInt160 publicKeyHash)
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<VerificationContract>();
            }

            return this.currentWallet.GetContracts(publicKeyHash);
        }

        public IEnumerable<Coin> GetCoins()
        {
            // TODO - ISSUE #37 [AboimPinto]: at this point the return should not be a object from the NEO assemblies but a DTO only know by the application with only the necessary fields.

            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<Coin>();
            }

            return this.currentWallet.GetCoins();
        }

        public IEnumerable<Coin> GetUnclaimedCoins()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<Coin>();
            }

            return this.currentWallet.GetUnclaimedCoins();
        }

        public IEnumerable<Coin> FindUnspentCoins()
        {
            if (!this.WalletIsOpen)
            {
                return Enumerable.Empty<Coin>();
            }

            return this.currentWallet.FindUnspentCoins();
        }

        public UInt160 GetChangeAddress()
        {
            return this.currentWallet?.GetChangeAddress();
        }

        public bool WalletContainsAddress(UInt160 scriptHash)
        {
            return this.WalletIsOpen && this.currentWallet.ContainsAddress(scriptHash);
        }

        public BigDecimal GetAvailable(UIntBase assetId)
        {
            if (!this.WalletIsOpen)
            {
                return new BigDecimal(BigInteger.Zero, 0);
            }

            return this.currentWallet.GetAvailable(assetId);
        }

        public Fixed8 GetAvailable(UInt256 assetId)
        {
            if (!this.WalletIsOpen)
            {
                return Fixed8.Zero;
            }

            return this.currentWallet.GetAvailable(assetId);
        }

        public VerificationContract GetContract(UInt160 scriptHash)
        {
            // TODO - ISSUE #37 [AboimPinto]: at this point the return should not be a object from the NEO assemblies but a DTO only know by the application with only the necessary fields.

            return currentWallet?.GetContract(scriptHash);
        }

        public void CreateNewKey()
        {
            var newKey = this.currentWallet.CreateKey();

            var contractsForKey = this.currentWallet.GetContracts(newKey.PublicKeyHash);
            foreach(var contract in contractsForKey)
            {
                this.AddContract(contract);
            }
        }

        public void ImportWatchOnlyAddress(string addressToImport)
        {
            using (var reader = new StringReader(addressToImport))
            {
                while (true)
                {
                    var address = reader.ReadLine();
                    if (address == null) break;
                    address = address.Trim();
                    if (string.IsNullOrEmpty(address)) continue;
                    UInt160 scriptHash;
                    try
                    {
                        scriptHash = Wallet.ToScriptHash(address);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }
                    this.currentWallet.AddWatchOnly(scriptHash);
                    this.AddAddress(scriptHash);
                }
            }
        }

        public void DeleteAccount(AccountItem accountToDelete)
        {
            if (accountToDelete == null)
            {
                throw new ArgumentNullException(nameof(accountToDelete));
            }

            var scriptHash = accountToDelete.ScriptHash != null
                ? accountToDelete.ScriptHash
                : accountToDelete.Contract.ScriptHash;

            this.currentWallet.DeleteAddress(scriptHash);
            this.accounts.Remove(accountToDelete);

            this.messagePublisher.Publish(new AccountItemsChangedMessage(this.accounts));
        }

        public Transaction MakeTransaction(Transaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8))
        {
            return this.currentWallet?.MakeTransaction(transaction);
        }

        public ContractTransaction MakeTransaction(ContractTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8))
        {
            return this.currentWallet?.MakeTransaction(transaction, changeAddress, fee);
        }

        public InvocationTransaction MakeTransaction(InvocationTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8))
        {
            return this.currentWallet?.MakeTransaction(transaction, changeAddress, fee);
        }

        public IssueTransaction MakeTransaction(IssueTransaction transaction, UInt160 changeAddress = null, Fixed8 fee = default(Fixed8))
        {
            return this.currentWallet?.MakeTransaction(transaction, changeAddress, fee);
        }

        #endregion

        #region IMessageHandler implementation 
        public void HandleMessage(AddContractsMessage message)
        {
            if (message.Contracts == null || !message.Contracts.Any())
            {
                return;
            }

            foreach (var contract in message.Contracts)
            {
                this.currentWallet.AddContract(contract);
                this.AddContract(contract);
            }
        }

        public void HandleMessage(AddContractMessage message)
        {
            if (message.Contract == null)
            {
                return;
            }

            this.currentWallet.AddContract(message.Contract);
            this.AddContract(message.Contract);
        }

        public void HandleMessage(ImportPrivateKeyMessage message)
        {
            if (message.WifStrings == null) return;

            if (!message.WifStrings.Any()) return;

            foreach (var wif in message.WifStrings)
            {
                KeyPair key;
                try
                {
                    key = this.currentWallet.Import(wif);
                }
                catch (FormatException)
                {
                    // Skip WIF line
                    continue;
                }
                foreach (var contract in this.currentWallet.GetContracts(key.PublicKeyHash))
                {
                    this.AddContract(contract);
                }
            }
        }

        public void HandleMessage(ImportCertificateMessage message)
        {
            if (message.SelectedCertificate == null) return;

            KeyPair key;
            try
            {
                key = this.currentWallet.Import(message.SelectedCertificate);
            }
            catch
            {
                //await DialogCoordinator.Instance.ShowMessageAsync(this, string.Empty, "Certificate import failed!");
                return;
            }

            foreach (var contract in this.currentWallet.GetContracts(key.PublicKeyHash))
            {
                this.AddContract(contract);
            }
        }

        public void HandleMessage(SignTransactionAndShowInformationMessage message)
        {
            var transaction = message.Transaction;

            if (transaction == null)
            {
                this.notificationHelper.ShowErrorNotification(Strings.InsufficientFunds);
                return;
            }

            ContractParametersContext context;
            try
            {
                context = new ContractParametersContext(transaction);
            }
            catch (InvalidOperationException)
            {
                this.notificationHelper.ShowErrorNotification(Strings.UnsynchronizedBlock);
                return;
            }

            this.Sign(context);

            if (context.Completed)
            {
                context.Verifiable.Scripts = context.GetScripts();

                this.SaveTransaction(transaction);
                this.blockChainController.Relay(transaction);

                this.notificationHelper.ShowSuccessNotification($"{Strings.SendTxSucceedMessage} {transaction.Hash.ToString()}");
            }
            else
            {
                this.notificationHelper.ShowSuccessNotification($"{Strings.IncompletedSignatureMessage} {context.ToString()}");
            }
        }
        #endregion

        #region Private Methods
        private void SetCurrentWallet(UserWallet wallet)
        {
            if (this.WalletIsOpen)
            {
                // Dispose current wallet
                this.currentWallet.BalanceChanged -= this.CurrentWalletBalanceChanged;
                this.currentWallet.TransactionsChanged -= this.CurrentWalletTransactionsChanged;
                this.currentWallet.Dispose();
            }

            this.messagePublisher.Publish(new ClearAccountsMessage());
            this.messagePublisher.Publish(new ClearAssetsMessage());
            this.messagePublisher.Publish(new ClearTransactionsMessage());

            this.currentWallet = wallet;

            if (this.WalletIsOpen)
            {
                // Setup wallet
                var transactions = this.currentWallet.LoadTransactions();
                this.messagePublisher.Publish(new UpdateTransactionsMessage(transactions));

                this.currentWallet.BalanceChanged += this.CurrentWalletBalanceChanged;
                this.currentWallet.TransactionsChanged += this.CurrentWalletTransactionsChanged;
            }

            this.messagePublisher.Publish(new CurrentWalletHasChangedMessage());
            this.LoadWallet();

            this.balanceChanged = true;
            this.checkNep5Balance = true;
        }

        private void CurrentWalletTransactionsChanged(object sender, IEnumerable<TransactionInfo> transactions)
        {
            this.messagePublisher.Publish(new UpdateTransactionsMessage(transactions));
        }

        private void CurrentWalletBalanceChanged(object sender, EventArgs e)
        {
            this.balanceChanged = true;
        }

        private void MigrateWallet(string walletPath)
        {
            var pathOld = Path.ChangeExtension(walletPath, ".old.db3");
            var pathNew = Path.ChangeExtension(walletPath, ".new.db3");
            UserWallet.Migrate(walletPath, pathNew);
            File.Move(walletPath, pathOld);
            File.Move(pathNew, walletPath);
        }

        private UserWallet OpenWalletWithPath(string walletPath, string password)
        {
            try
            {
                return UserWallet.Open(walletPath, password);
                
            }
            catch (CryptographicException)
            {
                //this.dialogHelper.ShowDialog("WalletPasswordIncorrectDialog", Strings.PasswordIncorrect);
            }

            return null;
        }

        private void LoadWallet()
        {
            if (!this.WalletIsOpen) return;

            foreach (var walletAddress in this.GetAddresses())
            {
                var contract = this.GetContract(walletAddress);
                if (contract == null)
                {
                    this.AddAddress(walletAddress);
                }
                else
                {
                    this.AddContract(contract);
                }
            }
        }

        private void AddAddress(UInt160 scriptHash)
        {
            var address = Wallet.ToAddress(scriptHash);
            var accountItemForAddress = this.accounts.GetAccountItemForAddress(address);

            if (accountItemForAddress == null)
            {
                var newAccountItem = new AccountItem
                {
                    Address = address,
                    Type = AccountType.WatchOnly,
                    Neo = Fixed8.Zero,
                    Gas = Fixed8.Zero,
                    ScriptHash = scriptHash
                };

                this.accounts.Add(newAccountItem);
            }

            this.messagePublisher.Publish(new AccountItemsChangedMessage(this.accounts));
        }

        private void AddContract(VerificationContract contract)
        {
            var accountItemForAddress = this.accounts.GetAccountItemForAddress(contract.Address);

            if (accountItemForAddress?.ScriptHash != null)          // [AboimPinto] what this logic mean?
            {
                this.accounts.Remove(accountItemForAddress);
            }

            if (accountItemForAddress == null)
            {
                var newAccountItem = new AccountItem
                {
                    Address = contract.Address,
                    Type = contract.IsStandard ? AccountType.Standard : AccountType.NonStandard,
                    Neo = Fixed8.Zero,
                    Gas = Fixed8.Zero,
                    Contract = contract
                };

                this.accounts.Add(newAccountItem);
            }

            this.messagePublisher.Publish(new AccountItemsChangedMessage(this.accounts));
        }
        #endregion
    }
}
