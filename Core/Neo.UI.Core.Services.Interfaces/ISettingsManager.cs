namespace Neo.UI.Core.Services.Interfaces
{
    public interface ISettingsManager
    {
        string LastWalletPath { get; set; }

        string AppThemeJson { get; set; }

        bool LightWalletMode { get; set; }

        string[] LightWalletRpcSeedList { get; }

        string CertificateCachePath { get; }

        string[] NEP5WatchScriptHashes { get; set; }

        #region Local Node Settings

        string BlockchainDataDirectoryPath { get; }

        int LocalNodePort { get; }

        int LocalWSPort { get; }

        #endregion

        #region URL Format Settings

        string AddressURLFormat { get; }

        string AssetURLFormat { get; }

        string TransactionURLFormat { get; }

        #endregion

        void Save();
    }
}
