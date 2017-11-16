using System.Collections;
using System.Collections.Generic;
using Neo.Wallets;

namespace Neo.Controllers
{
    public interface IWalletController
    {
        bool IsWalletOpen { get; }

        uint WalletWeight { get; }

        void CreateWallet(string walletPath, string password);

        void OpenWallet(string walletPath, string password, bool repairMode);

        IEnumerable<UInt160> GetAddresses();

        IEnumerable<Coin> GetCoins();

        VerificationContract GetContract(UInt160 scriptHash);
    }
}
