using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Theming;
using Neo.Gui.Wpf.Properties;

namespace Neo.Gui.Wpf.Helpers
{
    public class ThemeHelper : IThemeHelper
    {
        private const string CustomAccentKey = "CustomAccent";

        #region Resource Key Constants

        private const string HighlightColorKey = "ThemeHighlightColor";
        private const string AccentBaseColorKey = "ThemeAccentBaseColor";
        private const string AccentColor1Key = "ThemeAccentColor";
        private const string AccentColor2Key = "ThemeAccentColor2";
        private const string AccentColor3Key = "ThemeAccentColor3";
        private const string AccentColor4Key = "ThemeAccentColor4";
        private const string WindowBorderColorKey = "ThemeWindowBorderColor";

        #endregion Resource Key Constants

        #region IThemeHelper implementation

        public NeoGuiTheme CurrentTheme { get; private set; }

        public void LoadTheme()
        {
            // Add custom accent resource dictionary to the ThemeManager, contains default color values
            ThemeManager.AddAccent(CustomAccentKey, new Uri("pack://application:,,,/neo-gui;component/UI/Base/Resources/CustomAccentThemeResources.xaml"));


            // Try load custom theme
            var themeJson = Settings.Default.AppTheme;

            // Check if theme JSON has been set
            if (string.IsNullOrEmpty(themeJson))
            {
                SetThemeToDefault();
                return;
            }

            var theme = NeoGuiTheme.ImportFromJson(themeJson);

            if (theme == null)
            {
                theme = NeoGuiTheme.Default;
            }

            this.SetTheme(theme);
        }

        public void SetTheme(NeoGuiTheme newTheme)
        {
            // Change app style to the custom accent and current theme
            var accent = ThemeManager.GetAccent(CustomAccentKey);
            var theme = ThemeManager.GetAppTheme(newTheme.Style == ThemeStyle.Light ? "BaseLight" : "BaseDark");

            // Modify resource values to new theme values

            // Set accent colors
            if (accent.Resources.Contains(AccentBaseColorKey))
            {
                // Set base accent color
                accent.Resources[AccentBaseColorKey] = Color.FromArgb(newTheme.AccentBaseColor.A, newTheme.AccentBaseColor.R, newTheme.AccentBaseColor.G, newTheme.AccentBaseColor.B);


                // Set other accent colors with reduced transparency

                // Set 80% transparency accent color
                if (accent.Resources.Contains(AccentColor1Key))
                {
                    var accent1Color = newTheme.AccentBaseColor.SetTransparencyFraction(0.8);

                    accent.Resources[AccentColor1Key] = Color.FromArgb(accent1Color.A, accent1Color.R, accent1Color.G, accent1Color.B);
                }

                // Set 60% transparency accent color
                if (accent.Resources.Contains(AccentColor2Key))
                {
                    var accent2Color = newTheme.AccentBaseColor.SetTransparencyFraction(0.6);

                    accent.Resources[AccentColor2Key] = Color.FromArgb(accent2Color.A, accent2Color.R, accent2Color.G, accent2Color.B);
                }

                // Set 40% transparency accent color
                if (accent.Resources.Contains(AccentColor3Key))
                {
                    var accent3Color = newTheme.AccentBaseColor.SetTransparencyFraction(0.4);

                    accent.Resources[AccentColor3Key] = Color.FromArgb(accent3Color.A, accent3Color.R, accent3Color.G, accent3Color.B);
                }

                // Set 20% transparency accent color
                if (accent.Resources.Contains(AccentColor4Key))
                {
                    var accent4Color = newTheme.AccentBaseColor.SetTransparencyFraction(0.2);

                    accent.Resources[AccentColor4Key] = Color.FromArgb(accent4Color.A, accent4Color.R, accent4Color.G, accent4Color.B);
                }
            }

            // Set highlight color
            if (accent.Resources.Contains(HighlightColorKey))
            {
                accent.Resources[HighlightColorKey] = Color.FromArgb(newTheme.HighlightColor.A, newTheme.HighlightColor.R, newTheme.HighlightColor.G, newTheme.HighlightColor.B);
            }

            // Set window border color
            if (accent.Resources.Contains(WindowBorderColorKey))
            {
                accent.Resources[WindowBorderColorKey] = Color.FromArgb(newTheme.WindowBorderColor.A, newTheme.WindowBorderColor.R, newTheme.WindowBorderColor.G, newTheme.WindowBorderColor.B);
            }

            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);

            this.CurrentTheme = newTheme;
        }

        #endregion

        #region Private methods

        private void SetThemeToDefault()
        {
            this.SetTheme(NeoGuiTheme.Default);
        }

        #endregion
    }
}