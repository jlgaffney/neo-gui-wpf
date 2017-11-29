using System.Windows;
using MahApps.Metro.Controls;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Controls
{
    public class NeoWindow : MetroWindow
    {
        public NeoWindow()
        {
            this.BorderThickness = new Thickness(1);

            // Set color resource references
            this.SetResourceReference(BorderBrushProperty, "WindowBorderColorBrush");
            this.SetResourceReference(NonActiveWindowTitleBrushProperty, "AccentColorBrush2");
            this.SetResourceReference(NonActiveBorderBrushProperty, "WindowBorderColor2Brush");

            this.Loaded += this.WindowLoaded;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModelBase;

            viewModel?.OnWindowAttached(this);
        }
    }
}