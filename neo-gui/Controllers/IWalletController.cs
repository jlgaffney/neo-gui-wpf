namespace Neo.Controllers
{
    public interface IWalletController
    {
        void CreateWallet(string walletPath, string password);

        void OpenWallet(string walletPath, string password);
    }
}
