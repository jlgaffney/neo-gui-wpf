using System.Windows;
using System.Windows.Media;
using Autofac;
using MahApps.Metro.Controls;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Theming;
using Neo.Gui.Wpf.Extensions;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Controls
{
    public class NeoWindow : MetroWindow
    {
        private static ILifetimeScope containerLifetimeScope;

        public NeoWindow()
        {
            this.BorderThickness = new Thickness(1.0);

            var themeHelper = containerLifetimeScope?.Resolve <IThemeHelper>();

            var theme = themeHelper?.CurrentTheme == null ? NeoGuiTheme.Default : themeHelper.CurrentTheme;

            this.BorderBrush = new SolidColorBrush(theme.WindowBorderColor.ToMediaColor());

            this.NonActiveWindowTitleBrush = new SolidColorBrush(theme.AccentBaseColor.SetTransparencyFraction(0.8).ToMediaColor());

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