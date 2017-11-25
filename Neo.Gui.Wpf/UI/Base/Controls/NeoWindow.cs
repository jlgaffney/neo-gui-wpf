using System.Windows;
using System.Windows.Media;
using Autofac;
using MahApps.Metro.Controls;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Interfaces.Helpers;
using Neo.Gui.Base.Theming;
using Neo.UI.Base.MVVM;
using Neo.UI.Base.Themes;

namespace Neo.UI.Base.Controls
{
    public class NeoWindow : MetroWindow
    {
        private static ILifetimeScope containerLifetimeScope;

        public NeoWindow()
        {
            this.BorderThickness = new Thickness(1.0);

            var themeHelper = containerLifetimeScope?.Resolve <IThemeHelper>();

            var theme = themeHelper?.CurrentTheme == null ? NeoGuiTheme.Default : themeHelper.CurrentTheme;

            this.BorderBrush = new SolidColorBrush(Color.FromArgb(theme.WindowBorderColor.A, theme.WindowBorderColor.R, theme.WindowBorderColor.G, theme.WindowBorderColor.B));

            var nonActiveWindowTitleDrawingColor = theme.AccentBaseColor.SetTransparencyFraction(0.8);

            this.NonActiveWindowTitleBrush = new SolidColorBrush(Color.FromArgb(nonActiveWindowTitleDrawingColor.A, nonActiveWindowTitleDrawingColor.R, nonActiveWindowTitleDrawingColor.G, nonActiveWindowTitleDrawingColor.B));

            this.Loaded += (sender, e) =>
            {
                var viewModel = this.DataContext as ViewModelBase;

                viewModel?.OnWindowAttached(this);
            };            
        }

        public static void SetLifetimeScope(ILifetimeScope lifetimeScope)
        {
            containerLifetimeScope = lifetimeScope;
        }
    }
}