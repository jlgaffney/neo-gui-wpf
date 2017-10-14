using System;
using System.Windows;
using MahApps.Metro;
using Neo.Implementations.Wallets.EntityFramework;

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

            // Get the current app style (theme and accent) from the application
            var theme = ThemeManager.DetectAppStyle(Current);

            // Change app style to the custom accent and current theme
            ThemeManager.ChangeAppStyle(Current,
                ThemeManager.GetAccent("CustomAccent1"),
                theme.Item1);

            base.OnStartup(e);
        }
    }
}