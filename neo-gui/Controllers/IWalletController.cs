using System.Collections;
using System.Collections.Generic;
using Neo.Core;
using Neo.SmartContract;
using Neo.Wallets;

namespace Neo.Controllers
{
    public interface IWalletController
    {
        bool IsWalletOpen { get; }

        uint WalletWeight { get; }

        void CreateWallet(string walletPath, string password);

        void OpenWallet(string walletPath, string password, bool repairMode);

        void CloseWallet();

        void RebuildWalletIndexes();

        void SaveTransaction(Transaction transaction);

        void Sign(ContractParametersContext context);

        IEnumerable<UInt160> GetAddresses();

        IEnumerable<Coin> GetCoins();

        VerificationContract GetContract(UInt160 scriptHash);
    }
}
