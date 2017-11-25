using System.Windows.Input;

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
        }

        private void NeoSplashScreen_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            this.DragMove();
        }
    }
}