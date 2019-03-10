using System;
using System.IO;
using System.Security.Cryptography;
using Neo.Gui.Cross.Messages;
using Neo.Gui.Cross.Messaging;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using Neo.Wallets.SQLite;

namespace Neo.Gui.Cross.Services
{
    public class WalletService : IWalletService
    {
        private readonly IAccountBalanceService accountBalanceService;
        private readonly IMessageAggregator messageAggregator;
        private readonly ISettings settings;

        public WalletService(
            IAccountBalanceService accountBalanceService,
            IMessageAggregator messageAggregator,
            ISettings settings)
        {
            this.accountBalanceService = accountBalanceService;
            this.messageAggregator = messageAggregator;
            this.settings = settings;
        }

        private WalletIndexer indexer;


        public bool WalletIsOpen => CurrentWallet != null;

        public Wallet CurrentWallet { get; private set; }

        private WalletIndexer Indexer => indexer ?? (indexer = new WalletIndexer(settings.Paths.Index));






        public void CreateWallet(string filePath, string password)
        {
            var wallet = new NEP6Wallet(Indexer, filePath);
            wallet.Unlock(password);
            wallet.CreateAccount();
            wallet.Save();

            SetCurrentWallet(wallet);

            messageAggregator.Publish(new WalletOpenedMessage());
        }

        public bool OpenWallet(string filePath, string password, out string upgradeWalletPath)
        {
            upgradeWalletPath = null;

            var path = filePath;

            Wallet wallet;
            if (Path.GetExtension(path) == ".db3")
            {
                if (false)//MessageBox.Show(Strings.MigrateWalletMessage, Strings.MigrateWalletCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    var pathOld = path;
                    path = Path.ChangeExtension(pathOld, ".json");

                    // TODO Check if this path is available

                    NEP6Wallet nep6Wallet;
                    try
                    {
                        nep6Wallet = NEP6Wallet.Migrate(Indexer, path, pathOld, password);
                    }
                    catch (CryptographicException)
                    {
                        //      MessageBox.Show(Strings.PasswordIncorrect);
                        return false;
                    }
                    nep6Wallet.Save();
                    nep6Wallet.Unlock(password);
                    wallet = nep6Wallet;
                    //  MessageBox.Show($"{Strings.MigrateWalletSucceedMessage}\n{path}");
                }
                else
                {
                    try
                    {
                        wallet = UserWallet.Open(Indexer, path, password);
                    }
                    catch (CryptographicException)
                    {
                        // MessageBox.Show(Strings.PasswordIncorrect);
                        return false;
                    }
                }
            }
            else
            {
                var nep6Wallet = new NEP6Wallet(Indexer, path);
                try
                {
                    nep6Wallet.Unlock(password);
                }
                catch (CryptographicException)
                {
                    //MessageBox.Show(Strings.PasswordIncorrect);
                    return false;
                }
                wallet = nep6Wallet;
            }

            if (path != filePath)
            {
                upgradeWalletPath = path;
            }

            SetCurrentWallet(wallet);

            messageAggregator.Publish(new WalletOpenedMessage());

            return true;
        }

        public void SaveWallet()
        {
            if (!(CurrentWallet is NEP6Wallet wallet))
            {
                return;
            }

            wallet.Save();
        }

        public void CloseWallet()
        {
            if (!WalletIsOpen)
            {
                return;
            }

            CurrentWallet.WalletTransaction -= CurrentWallet_WalletTransaction;

            if (CurrentWallet is IDisposable disposable)
            {
                disposable.Dispose();
            }

            CurrentWallet = null;

            messageAggregator.Publish(new WalletClosedMessage());
        }


        private void SetCurrentWallet(Wallet wallet)
        {
            if (WalletIsOpen)
            {
                CloseWallet();
            }

            CurrentWallet = wallet;

            accountBalanceService.Clear();
            accountBalanceService.GlobalAssetBalanceChanged = true;
            accountBalanceService.NEP5TokenBalanceChanged = true;

            if (CurrentWallet != null)
            {
                CurrentWallet.WalletTransaction += CurrentWallet_WalletTransaction;
            }
        }

        private void CurrentWallet_WalletTransaction(object sender, WalletTransactionEventArgs e)
        {
            accountBalanceService.GlobalAssetBalanceChanged = true;
        }
    }
}
