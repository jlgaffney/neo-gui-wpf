namespace Neo.UI.Core.Wallet.Initialization
{
    public class FullWalletInitializationParameters : IWalletInitializationParameters
    {
        public FullWalletInitializationParameters(int localNodePort, int localWSPort,
            string blockchainDataDirectoryPath, string certificateCachePath)
        {
            this.LocalNodePort = localNodePort;
            this.LocalWSPort = localWSPort;

            this.BlockchainDataDirectoryPath = blockchainDataDirectoryPath;
            this.CertificateCachePath = certificateCachePath;
        }

        internal int LocalNodePort { get; }

        internal int LocalWSPort { get; }

        internal string BlockchainDataDirectoryPath { get; }

        internal string CertificateCachePath { get; }
    }
}
