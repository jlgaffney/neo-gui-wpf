using System.Windows;
using Neo.UI.Base.Themes;

namespace Neo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        internal App()
        {
            this.InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeHelper.LoadTheme();

            base.OnStartup(e);
        }
    }
}