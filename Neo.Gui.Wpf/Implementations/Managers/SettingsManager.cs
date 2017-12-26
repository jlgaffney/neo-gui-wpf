using Neo.Gui.Base.Managers;
using Neo.Gui.Wpf.Extensions;
using Neo.Gui.Wpf.Properties;

namespace Neo.Gui.Wpf.Implementations.Managers
{
    public class SettingsManager : ISettingsManager
    {
        public string LastWalletPath
        {
            get => Settings.Default.LastWalletPath;
            set => Settings.Default.LastWalletPath = value;
        }

        public string AppTheme
        {
            get => Settings.Default.AppTheme;
            set => Settings.Default.AppTheme = value;
        }

        public bool RemoteNodeMode => Settings.Default.P2P.RemoteNodeMode;

        public string CertificateCachePath => Settings.Default.Paths.CertCache;

        public string[] NEP5WatchScriptHashes
        {
            get => Settings.Default.NEP5Watched.ToArray();
            set
            {
                Settings.Default.NEP5Watched.Clear();
                Settings.Default.NEP5Watched.AddRange(value);
            }
        }

        #region Local Node Settings

        public string BlockchainDataDirectoryPath => Settings.Default.Paths.Chain;

        public int LocalNodePort => Settings.Default.P2P.Port;

        public int LocalWSPort => Settings.Default.P2P.WsPort;

        #endregion

        #region URL Format Settings

        public string AddressURLFormat => Settings.Default.Urls.AddressUrl;

        public string AssetURLFormat => Settings.Default.Urls.AssetUrl;

        public string TransactionURLFormat => Settings.Default.Urls.TransactionUrl;

        #endregion

        public void Save()
        {
            Settings.Default.Save();
        }
    }
}
