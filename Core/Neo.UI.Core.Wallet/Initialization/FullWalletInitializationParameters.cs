namespace Neo.UI.Core.Wallet.Initialization
{
    public class FullWalletInitializationParameters : BaseWalletInitializationParameters
    {
        public FullWalletInitializationParameters(int localNodePort, int localWSPort,
            string blockchainDataDirectoryPath, string certificateCachePath) : base(certificateCachePath)
        {
            this.LocalNodePort = localNodePort;
            this.LocalWSPort = localWSPort;

            this.BlockchainDataDirectoryPath = blockchainDataDirectoryPath;
        }

        internal int LocalNodePort { get; }

        internal int LocalWSPort { get; }

        internal string BlockchainDataDirectoryPath { get; }
    }
}
