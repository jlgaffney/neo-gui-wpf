using System;
using System.Collections.Generic;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Properties;
using Neo.UI.Base.Messages;
using Neo.UI.Messages;

namespace Neo.Controllers
{
    public class WalletController : IWalletController
    {
        #region Private Fields 
        private readonly IApplicationContext applicationContext;
        private readonly IMessagePublisher messagePublisher;

        private UserWallet currentWallet;

        private bool balanceChanged;
        private bool checkNep5Balance;
        #endregion

        #region Constructor 
        public WalletController(
            IApplicationContext applicationContext,
            IMessagePublisher messagePublisher)
        {
            this.applicationContext = applicationContext;
            this.messagePublisher = messagePublisher;
        }
        #endregion

        #region IWalletController implementation 
        public void CreateWallet(string walletPath, string password)
        {
            var newWallet = UserWallet.Create(walletPath, password);

            this.SetCurrentWallet(newWallet);

            Settings.Default.LastWalletPath = walletPath;
            Settings.Default.Save();
        }

        public void OpenWallet(string walletPath, string password)
        {
            throw new NotImplementedException();
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
        #endregion
    }
}
