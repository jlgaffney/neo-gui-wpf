using Neo.Network.P2P.Payloads;
using Neo.Wallets;

namespace Neo.Gui.Cross.Services
{
    public interface IWalletService
    {
        bool WalletIsOpen { get; }

        Wallet CurrentWallet { get; }

        
        void CreateWallet(string filePath, string password);

        bool OpenWallet(string filePath, string password, out string upgradedWalletPath);

        
        void SaveWallet();

        void CloseWallet();


        bool SignTransaction(Transaction transaction);
    }
}
