using System;
using System.Windows;
using MahApps.Metro;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Properties;
using Neo.UI;
using Neo.UI.Options;

namespace Neo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static UserWallet CurrentWallet;

        internal App()
        {
            this.InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Add custom accent and theme resource dictionaries to the ThemeManager
            ThemeManager.AddAccent("CustomAccent1", new Uri("pack://application:,,,/neo-gui;component/UI/Resources/ThemeResources.xaml"));

            var themeSetting = (NeoTheme)Enum.ToObject(typeof(NeoTheme), Settings.Default.AppTheme);

           SetTheme(themeSetting);

            base.OnStartup(e);
        }

        public static void SetTheme(NeoTheme appTheme)
        {
            // Change app style to the custom accent and current theme
            var accent = ThemeManager.GetAccent("CustomAccent1");
            var theme = ThemeManager.GetAppTheme(appTheme == NeoTheme.Light ? "BaseLight" : "BaseDark");

            ThemeManager.ChangeAppStyle(Current,
                accent,
                theme);
        }
    }
}