using System.Windows.Input;

namespace Neo.Gui.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void SplashScreen_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            this.DragMove();
        }
    }
}