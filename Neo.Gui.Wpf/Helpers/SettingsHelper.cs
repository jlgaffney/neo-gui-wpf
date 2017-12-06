using Neo.Gui.Wpf.Properties;

namespace Neo.Gui.Wpf.Helpers
{
    public class SettingsHelper : ISettingsHelper
    {
        public string LastWalletPath
        {
            get
            {
                return Settings.Default.LastWalletPath;
            }
            set
            {
                Settings.Default.LastWalletPath = value;
                Settings.Default.Save();
            }
        }
    }
}
