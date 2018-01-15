using System;
using System.Windows;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.Gui.Wpf.Extensions;
using Neo.UI.Core.Extensions;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Theming;
using MahAppsThemeManager = MahApps.Metro.ThemeManager;
using Style = Neo.UI.Core.Theming.Style;

namespace Neo.Gui.Wpf.Implementations.Managers
{
    public class ThemeManager : IThemeManager
    {
        #region Resource Key Constants

        private const string CustomAccentKey = "CustomAccent";

        private const string HighlightColorKey = "ThemeHighlightColor";
        private const string AccentBaseColorKey = "ThemeAccentBaseColor";
        private const string AccentColor1Key = "ThemeAccentColor";
        private const string AccentColor2Key = "ThemeAccentColor2";
        private const string AccentColor3Key = "ThemeAccentColor3";
        private const string AccentColor4Key = "ThemeAccentColor4";
        private const string WindowBorderColorKey = "ThemeWindowBorderColor";
        private const string WindowBorderColor2Key = "ThemeWindowBorderColor2";

        #endregion Resource Key Constants

        #region Private Fields

        private readonly ISettingsManager settingsManager;

        #endregion Private Fields

        #region Constructor

        public ThemeManager(
            ISettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
        }

        #endregion Constructor

        #region IThemeManager implementation

        public Theme CurrentTheme { get; private set; }

        public void LoadTheme()
        {
            // Add custom accent resource dictionary to the ThemeManager, contains default color values
            MahAppsThemeManager.AddAccent(CustomAccentKey, new Uri("pack://application:,,,/neo-gui;component/ResourceDictionaries/CustomAccentThemeResources.xaml"));


            // Try load custom theme
            var themeJson = settingsManager.AppThemeJson;

            // Check if theme JSON has been set
            if (string.IsNullOrEmpty(themeJson))
            {
                SetThemeToDefault();
                return;
            }

            var theme = Theme.ImportFromJson(themeJson);

            if (theme == null)
            {
                theme = Theme.Default;
            }

            this.SetTheme(theme);
        }

        public void SetTheme(Theme newTheme)
        {
            // Change app style to the custom accent and current theme
            var accent = MahAppsThemeManager.GetAccent(CustomAccentKey);
            var theme = MahAppsThemeManager.GetAppTheme(newTheme.Style == Style.Light ? "BaseLight" : "BaseDark");

            // Modify resource values to new theme values

            // Set accent colors
            if (accent.Resources.Contains(AccentBaseColorKey))
            {
                // Set base accent color
                accent.Resources[AccentBaseColorKey] = newTheme.AccentBaseColor.ToMediaColor();


                // Set other accent colors with reduced transparency

                // Set 80% transparency accent color
                if (accent.Resources.Contains(AccentColor1Key))
                {
                    accent.Resources[AccentColor1Key] = newTheme.AccentBaseColor.SetTransparencyFraction(0.8).ToMediaColor();
                }

                // Set 60% transparency accent color
                if (accent.Resources.Contains(AccentColor2Key))
                {
                    accent.Resources[AccentColor2Key] = newTheme.AccentBaseColor.SetTransparencyFraction(0.6).ToMediaColor();
                }

                // Set 40% transparency accent color
                if (accent.Resources.Contains(AccentColor3Key))
                {
                    accent.Resources[AccentColor3Key] = newTheme.AccentBaseColor.SetTransparencyFraction(0.4).ToMediaColor();
                }

                // Set 20% transparency accent color
                if (accent.Resources.Contains(AccentColor4Key))
                {
                    accent.Resources[AccentColor4Key] = newTheme.AccentBaseColor.SetTransparencyFraction(0.2).ToMediaColor();
                }
            }

            // Set highlight color
            if (accent.Resources.Contains(HighlightColorKey))
            {
                accent.Resources[HighlightColorKey] = newTheme.HighlightColor.ToMediaColor();
            }

            // Set window border color
            if (accent.Resources.Contains(WindowBorderColorKey))
            {
                accent.Resources[WindowBorderColorKey] = newTheme.WindowBorderColor.ToMediaColor();

                // Set 80% transparency window border color
                if (accent.Resources.Contains(WindowBorderColor2Key))
                {
                    accent.Resources[WindowBorderColor2Key] = newTheme.WindowBorderColor.SetTransparencyFraction(0.8).ToMediaColor();
                }
            }

            MahAppsThemeManager.ChangeAppStyle(Application.Current, accent, theme);

            this.CurrentTheme = newTheme;
        }

        #endregion

        #region Private methods

        private void SetThemeToDefault()
        {
            this.SetTheme(Theme.Default);
        }

        #endregion
    }
}