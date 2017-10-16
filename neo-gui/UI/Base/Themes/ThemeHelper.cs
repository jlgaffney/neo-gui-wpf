using System;
using System.Drawing;
using System.Windows;
using MahApps.Metro;

using Neo.Properties;
using Neo.UI.Base.Helpers;

namespace Neo.UI.Base.Themes
{
    public class ThemeHelper
    {
        private const string CustomAccentKey = "CustomAccent";

        #region Resource Key Constants

        private const string HighlightColorKey = "HighlightColor";
        private const string AccentBaseColorKey = "AccentBaseColor";
        private const string AccentColor1Key = "AccentColor";
        private const string AccentColor2Key = "AccentColor2";
        private const string AccentColor3Key = "AccentColor3";
        private const string AccentColor4Key = "AccentColor4";

        #endregion Resource Key Constants

        public static readonly NeoTheme DefaultTheme = new NeoTheme
        {
            Style = ThemeStyle.Light,
            HighlightColor = ColorHelper.HexToColor("#76B466"),
            AccentBaseColor = ColorHelper.HexToColor("#3DA43C"),
            WindowBorderColor = ColorHelper.HexToColor("#9EAF99")
        };

        public static void LoadTheme()
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

            var theme = NeoTheme.Import(themeJson);

            if (theme == null)
            {
                SetThemeToDefault();
                return;
            }

            theme.SetAsCurrentTheme();
        }

        private static void SetThemeToDefault()
        {
            DefaultTheme.SetAsCurrentTheme();
        }

        internal static void SetTheme(NeoTheme newTheme)
        {
            // Change app style to the custom accent and current theme
            var accent = ThemeManager.GetAccent(CustomAccentKey);
            var theme = ThemeManager.GetAppTheme(newTheme.Style == ThemeStyle.Light ? "BaseLight" : "BaseDark");

            // Modify resource values to new theme values

            // Set accent colors
            if (accent.Resources.Contains(AccentBaseColorKey))
            {
                // Set base accent color
                accent.Resources[AccentBaseColorKey] = newTheme.AccentBaseColor;


                // Set other accent colors with reduced transparency

                // Set 80% transparency accent color
                if (accent.Resources.Contains(AccentColor1Key))
                {
                    var accent1Color = ColorHelper.SetTransparencyFraction(newTheme.AccentBaseColor, 0.8);
                    
                    accent.Resources[AccentColor1Key] = accent1Color;
                }

                // Set 60% transparency accent color
                if (accent.Resources.Contains(AccentColor2Key))
                {
                    var accent2Color = ColorHelper.SetTransparencyFraction(newTheme.AccentBaseColor, 0.6);

                    accent.Resources[AccentColor2Key] = accent2Color;
                }

                // Set 40% transparency accent color
                if (accent.Resources.Contains(AccentColor3Key))
                {
                    var accent3Color = ColorHelper.SetTransparencyFraction(newTheme.AccentBaseColor, 0.4);

                    accent.Resources[AccentColor3Key] = accent3Color;
                }

                // Set 20% transparency accent color
                if (accent.Resources.Contains(AccentColor4Key))
                {
                    var accent4Color = ColorHelper.SetTransparencyFraction(newTheme.AccentBaseColor, 0.2);

                    accent.Resources[AccentColor4Key] = accent4Color;
                }
            }

            // Set highlight color
            if (accent.Resources.Contains(HighlightColorKey))
            {
                accent.Resources[HighlightColorKey] = newTheme.HighlightColor;
            }

            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
        }
    }
}