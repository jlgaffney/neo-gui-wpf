using System.Windows.Input;
using System.Windows.Media;
using Neo.UI.Base.Themes;

namespace Neo.UI
{
    /// <summary>
    /// Interaction logic for NeoSplashScreen.xaml
    /// </summary>
    public partial class NeoSplashScreen
    {
        public NeoSplashScreen()
        {
            InitializeComponent();

            this.Border.BorderBrush = new SolidColorBrush(NeoTheme.Current.WindowBorderColor);
        }

        private void NeoSplashScreen_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            this.DragMove();
        }
    }
}