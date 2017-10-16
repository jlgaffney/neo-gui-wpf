using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Neo.UI.Base.Helpers;
using Neo.UI.Base.MVVM;
using Neo.UI.Base.Themes;

namespace Neo.UI.Base.Controls
{
    public class NeoWindow : MetroWindow
    {
        public NeoWindow()
        {
            this.BorderThickness = new Thickness(1.0);

            var theme = NeoTheme.Current == null ? ThemeHelper.DefaultTheme : NeoTheme.Current;

            this.BorderBrush = new SolidColorBrush(theme.WindowBorderColor);

            this.NonActiveWindowTitleBrush = new SolidColorBrush(ColorHelper.SetTransparencyFraction(theme.AccentBaseColor, 0.8));

            this.Loaded += (sender, e) =>
            {
                var viewModel = this.DataContext as ViewModelBase;

                viewModel?.OnWindowAttached(this);
            };            
        }
    }
}