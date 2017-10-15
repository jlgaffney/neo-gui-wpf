using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Base.Controls
{
    public class NeoWindow : MetroWindow
    {
        private static readonly BrushConverter brushConverter = new BrushConverter();

        public NeoWindow()
        {
            this.BorderThickness = new Thickness(1.0);
            
            this.BorderBrush = (Brush) brushConverter.ConvertFromString("#9EAF99");

            this.NonActiveWindowTitleBrush = (Brush) brushConverter.ConvertFromString("#89B27E");

            this.Loaded += (sender, e) =>
            {
                var viewModel = this.DataContext as ViewModelBase;

                viewModel?.OnWindowAttached(this);
            };            
        }
    }
}