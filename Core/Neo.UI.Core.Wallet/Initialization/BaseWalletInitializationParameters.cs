namespace Neo.UI.Core.Wallet.Initialization
{
    public abstract class BaseWalletInitializationParameters : IWalletInitializationParameters
    {
        internal BaseWalletInitializationParameters(
            string certificateCachePath)
        {
            this.CertificateCachePath = certificateCachePath;
        }

        internal string CertificateCachePath { get; }
    }
}
