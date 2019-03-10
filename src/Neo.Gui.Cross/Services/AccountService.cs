using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Neo.Cryptography.ECC;
using Neo.Gui.Cross.Exceptions;
using Neo.Gui.Cross.Extensions;
using Neo.Gui.Cross.Models;
using Neo.SmartContract;
using Neo.Wallets;

namespace Neo.Gui.Cross.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountBalanceService accountBalanceService;
        private readonly IContractCreator contractCreator;
        private readonly IWalletService walletService;

        public AccountService(
            IAccountBalanceService accountBalanceService,
            IContractCreator contractCreator,
            IWalletService walletService)
        {
            this.accountBalanceService = accountBalanceService;
            this.contractCreator = contractCreator;
            this.walletService = walletService;
        }

        public IEnumerable<WalletAccount> GetAllAccounts()
        {
            ThrowIfWalletNotOpen();

            return walletService.CurrentWallet.GetAccounts();
        }

        public IEnumerable<WalletAccount> GetStandardAccounts()
        {
            ThrowIfWalletNotOpen();

            return walletService.CurrentWallet.GetAccounts().Where(
                account => account.GetAccountType() == AccountType.Standard);
        }

        public IEnumerable<WalletAccount> GetNonWatchOnlyAccounts()
        {
            ThrowIfWalletNotOpen();

            return walletService.CurrentWallet.GetAccounts().Where(
                account => !account.WatchOnly);
        }

        public WalletAccount CreateStandardAccount()
        {
            ThrowIfWalletNotOpen();

            var account = walletService.CurrentWallet.CreateAccount();

            if (account != null)
            {
                walletService.SaveWallet();
            }

            return account;
        }

        public WalletAccount CreateLockContractAccount(string publicKeyString, DateTime unlockTime)
        {
            ThrowIfWalletNotOpen();

            var publicKey = ECPoint.Parse(publicKeyString, ECCurve.Secp256r1);

            var contract = contractCreator.GetLockAccountContract(publicKey, unlockTime);

            var account = walletService.CurrentWallet.CreateAccount(contract, walletService.CurrentWallet.GetAccount(publicKey).GetKey());

            if (account != null)
            {
                walletService.SaveWallet();
            }

            return account;
        }

        public WalletAccount CreateMultiSignatureContractAccount(int minimumSignatures, IEnumerable<string> publicKeyStrings)
        {
            ThrowIfWalletNotOpen();

            var publicKeys = new HashSet<ECPoint>(publicKeyStrings.Select(publicKeyString => ECPoint.Parse(publicKeyString, ECCurve.Secp256r1)));

            var contract = contractCreator.GetMultiSignatureContract(minimumSignatures, publicKeys);

            var account = walletService.CurrentWallet.CreateAccount(contract, walletService.CurrentWallet.GetAccounts()
                .FirstOrDefault(p => p.HasKey && publicKeys.Contains(p.GetKey().PublicKey))?.GetKey());
            
            if (account != null)
            {
                walletService.SaveWallet();
            }

            return account;
        }

        public WalletAccount ImportPrivateKey(string wif)
        {
            ThrowIfWalletNotOpen();

            var account = walletService.CurrentWallet.Import(wif);
            
            if (account != null)
            {
                walletService.SaveWallet();
            }

            return account;
        }

        public WalletAccount ImportContract(Contract contract, byte[] privateKey)
        {
            ThrowIfWalletNotOpen();

            var account = walletService.CurrentWallet.CreateAccount(contract, privateKey);

            if (account != null)
            {
                walletService.SaveWallet();
            }

            return account;
        }

        public WalletAccount ImportCertificate(X509Certificate2 certificate)
        {
            ThrowIfWalletNotOpen();

            var account = walletService.CurrentWallet.Import(certificate);
            
            if (account != null)
            {
                walletService.SaveWallet();
            }

            return account;
        }

        public bool DeleteAccount(UInt160 scriptHash)
        {
            ThrowIfWalletNotOpen();

            if (walletService.CurrentWallet.DeleteAccount(scriptHash))
            {
                walletService.SaveWallet();

                accountBalanceService.GlobalAssetBalanceChanged = true;
                accountBalanceService.NEP5TokenBalanceChanged = true;
                return true;
            }

            return false;
        }

        private void ThrowIfWalletNotOpen()
        {
            if (!walletService.WalletIsOpen)
            {
                throw new WalletNotOpenException();
            }
        }
    }
}
