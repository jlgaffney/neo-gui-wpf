using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Neo.DialogResults;
using Neo.Helpers;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Properties;
using Neo.UI.Base.Messages;
using Neo.UI.Messages;
using Neo.Wallets;

namespace Neo.Controllers
{
    public class WalletController : IWalletController
    {
        #region Private Fields 
        private readonly IDialogHelper dialogHelper;
        private readonly IApplicationContext applicationContext;
        private readonly IMessagePublisher messagePublisher;

        private UserWallet currentWallet;

        private bool balanceChanged;
        private bool checkNep5Balance;
        #endregion

        #region Constructor 
        public WalletController(
            IDialogHelper dialogHelper,
            IApplicationContext applicationContext,
            IMessagePublisher messagePublisher)
        {
            this.dialogHelper = dialogHelper;
            this.applicationContext = applicationContext;
            this.messagePublisher = messagePublisher;
        }
        #endregion

        #region IWalletController implementation 
        public bool IsWalletOpen => this.currentWallet != null;

        public uint WalletWeight => this.currentWallet.WalletHeight;

        public void CreateWallet(string walletPath, string password)
        {
            var newWallet = UserWallet.Create(walletPath, password);

            this.SetCurrentWallet(newWallet);

            Settings.Default.LastWalletPath = walletPath;
            Settings.Default.Save();
        }

        public void OpenWallet(string walletPath, string password)
        {
            var openWalletDialogResult = this.dialogHelper.ShowDialog<OpenWalletDialogResult>("OpenWalletDialog");

            // [TODO] why this verification? Why the magic string?
            if (UserWallet.GetVersion(walletPath) < Version.Parse("1.3.5"))
            {
                var migrationApproved = this.dialogHelper.ShowDialog<YesOrNoDialogResult>("ApproveWalletMigrationDialog");

                if (!migrationApproved.Result.Yes)
                {
                    return;
                }

                this.MigrateWallet(walletPath);
                this.dialogHelper.ShowDialog("WalletMigrationCompleteDialog");
            }

            var userWallet = this.OpenWalletWithPath(walletPath, password);
            if (userWallet == null)
            {
                return;
            }

            if (openWalletDialogResult.Result.OpenInRepairMode)
            {
                userWallet.Rebuild();
            }
            this.SetCurrentWallet(userWallet);

            Settings.Default.LastWalletPath = walletPath;
            Settings.Default.Save();
        }

        public IEnumerable<UInt160> GetAddresses()
        {
            return this.currentWallet.GetAddresses();
        }

        public IEnumerable<Coin> GetCoins()
        {
            // TODO - ISSUE #37 [AboimPinto]: at this point the return should not be a object from the NEO assemblies but a DTO only know by the application with only the necessary fields.
            return this.currentWallet.GetCoins();
        }
        #endregion

        #region Private Methods
        private void SetCurrentWallet(UserWallet wallet)
        {
            if (this.currentWallet != null)
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

            if (this.currentWallet != null)
            {
                // Setup wallet
                var transactions = this.currentWallet.LoadTransactions();
                this.messagePublisher.Publish(new UpdateTransactionsMessage(transactions));

                this.currentWallet.BalanceChanged += this.CurrentWalletBalanceChanged;
                this.currentWallet.TransactionsChanged += this.CurrentWalletTransactionsChanged;
            }

            this.messagePublisher.Publish(new EnableMenuItemsMessage());
            this.messagePublisher.Publish(new LoadWalletAddressesMessage());

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
                this.dialogHelper.ShowDialog("WalletPasswordIncorrectDialog", Strings.PasswordIncorrect);
            }

            return null;
        }
        #endregion
    }
}
