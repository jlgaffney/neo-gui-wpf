using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Theming;
using Neo.Gui.Wpf.Extensions;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        private static readonly SolidColorBrush TransparentBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        private readonly IProcessHelper processHelper;
        private readonly IThemeHelper themeHelper;

        private string currentNEP5ContractsList;
        private string nep5ContractsList;

        private Style currentStyle;
        private Style selectedStyle;

        private string currentThemeAccentBaseColorHex;
        private string themeAccentBaseColorHex;

        private string currentThemeHighlightColorHex;
        private string themeHighlightColorHex;

        private string currentThemeWindowBorderColorHex;
        private string themeWindowBorderColorHex;


        public SettingsViewModel(
            IProcessHelper processHelper,
            IThemeHelper themeHelper)
        {
            this.processHelper = processHelper;
            this.themeHelper = themeHelper;

            this.LoadSettings();
        }

        private void LoadSettings()
        {
            this.LoadNEP5Settings();
            this.LoadAppearanceSettings();

            NotifyPropertyChanged(nameof(this.NEP5SettingsChanged));
            NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
        }

        private void LoadNEP5Settings()
        {
            var nep5ContractsLines = Properties.Settings.Default.NEP5Watched.OfType<string>().ToArray();

            // Concatenate lines
            var contractsList = string.Empty;

            foreach (var line in nep5ContractsLines)
            {
                contractsList += line + "\n";
            }

            this.currentNEP5ContractsList = contractsList;
            this.NEP5ContractsList = contractsList;
        }

        private void LoadAppearanceSettings()
        {
            var currentTheme = this.themeHelper.CurrentTheme;

            // Set theme style
            this.currentStyle = currentTheme.Style;
            this.SelectedStyle = currentTheme.Style;

            // Set color values, without transparency as these colors do not require transparency
            var accentBaseColorHex = currentTheme.AccentBaseColor.ToHex();
            this.currentThemeAccentBaseColorHex = accentBaseColorHex;
            this.ThemeAccentBaseColorHex = accentBaseColorHex;

            var highlightColorHex = currentTheme.HighlightColor.ToHex();
            this.currentThemeHighlightColorHex = highlightColorHex;
            this.ThemeHighlightColorHex = highlightColorHex;

            var windowBorderColorHex = currentTheme.WindowBorderColor.ToHex();
            this.currentThemeWindowBorderColorHex = windowBorderColorHex;
            this.ThemeWindowBorderColorHex = windowBorderColorHex;
        }

        #region NEP-5 Properties & Commands

        public string NEP5ContractsList
        {
            get => this.nep5ContractsList;
            set
            {
                if (this.nep5ContractsList == value) return;

                this.nep5ContractsList = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.NEP5SettingsChanged));
            }
        }

        public bool NEP5SettingsChanged => this.currentNEP5ContractsList != this.NEP5ContractsList;

        public ICommand SaveNEP5SettingsCommand => new RelayCommand(this.SaveNEP5Settings);

        #endregion NEP-5 Properties & Commands

        #region Appearance Properties

        public Style[] Styles => Enum.GetValues(typeof(Style)).Cast<Style>().ToArray();

        public Style SelectedStyle
        {
            get => this.selectedStyle;
            set
            {
                if (this.selectedStyle == value) return;

                this.selectedStyle = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
            }
        }

        public string ThemeAccentBaseColorHex
        {
            get => this.themeAccentBaseColorHex;
            set
            {
                if (this.themeAccentBaseColorHex == value) return;

                this.themeAccentBaseColorHex = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));

                NotifyPropertyChanged(nameof(this.ThemeAccentBaseColor));
            }
        }

        // TODO Make this property a System.Drawing.Color
        // and use a type converter to convert it to a SolidColorBrush
        public SolidColorBrush ThemeAccentBaseColor
        {
            get
            {
                if (string.IsNullOrEmpty(this.ThemeAccentBaseColorHex)) return TransparentBrush;

                var accentBaseColor = this.ThemeAccentBaseColorHex.HexToColor();

                return new SolidColorBrush(accentBaseColor.SetTransparencyFraction(1.0).ToMediaColor());
            }
        }

        public string ThemeHighlightColorHex
        {
            get => this.themeHighlightColorHex;
            set
            {
                if (this.themeHighlightColorHex == value) return;

                this.themeHighlightColorHex = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));

                NotifyPropertyChanged(nameof(this.ThemeHighlightColor));
            }
        }

        // TODO Make this property a System.Drawing.Color
        // and use a type converter to convert it to a SolidColorBrush
        public SolidColorBrush ThemeHighlightColor
        {
            get
            {
                if (string.IsNullOrEmpty(this.ThemeHighlightColorHex)) return TransparentBrush;

                var highlightColor = this.ThemeHighlightColorHex.HexToColor();

                return new SolidColorBrush(highlightColor.SetTransparencyFraction(1.0).ToMediaColor());
            }
        }

        public string ThemeWindowBorderColorHex
        {
            get => this.themeWindowBorderColorHex;
            set
            {
                if (this.themeWindowBorderColorHex == value) return;

                this.themeWindowBorderColorHex = value;

                NotifyPropertyChanged();

                // Update dependent properties
                NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));

                NotifyPropertyChanged(nameof(this.ThemeWindowBorderColor));
            }
        }

        // TODO Make this property a System.Drawing.Color
        // and use a type converter to convert it to a SolidColorBrush
        public SolidColorBrush ThemeWindowBorderColor
        {
            get
            {
                if (string.IsNullOrEmpty(this.ThemeWindowBorderColorHex)) return TransparentBrush;

                var windowBorderColor = this.ThemeWindowBorderColorHex.HexToColor();

                return new SolidColorBrush(windowBorderColor.SetTransparencyFraction(1.0).ToMediaColor());
            }
        }

        public bool AppearanceSettingsChanged =>
            this.currentStyle != this.SelectedStyle ||
            this.currentThemeAccentBaseColorHex != this.ThemeAccentBaseColorHex ||
            this.currentThemeHighlightColorHex != this.ThemeHighlightColorHex ||
            this.currentThemeWindowBorderColorHex != this.ThemeWindowBorderColorHex;

        public ICommand ResetAppearanceSettingsToDefaultCommand => new RelayCommand(this.ResetAppearanceSettingsToDefault);

        public ICommand SaveAppearanceSettingsCommand => new RelayCommand(this.SaveAppearanceSettings);

        public ICommand ApplyAppearanceSettingsCommand => new RelayCommand(this.ApplyAppearanceSettings);

        #endregion Appearance Properties

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        private void Ok()
        {
            if (this.NEP5SettingsChanged || this.AppearanceSettingsChanged)
            {
                // TODO Show message warning user their settings
                // changes will be discarded if they continue
            }

            this.TryClose();
        }

        private void Cancel()
        {
            this.TryClose();
        }

        private void SaveNEP5Settings()
        {
            var nep5ContractsLines =  string.IsNullOrEmpty(this.NEP5ContractsList)
                ? new string[0] : this.NEP5ContractsList.ToLines();

            Properties.Settings.Default.NEP5Watched.Clear();
            Properties.Settings.Default.NEP5Watched.AddRange(nep5ContractsLines.Where(p =>
                !string.IsNullOrWhiteSpace(p) && UInt160.TryParse(p, out _)).ToArray());

            Properties.Settings.Default.Save();

            
            // Update settings' current values
            this.currentNEP5ContractsList = this.NEP5ContractsList;
            NotifyPropertyChanged(nameof(this.NEP5SettingsChanged));
        }

        private async void ApplyAppearanceSettings()
        {
            var restartApprovedResult = await DialogCoordinator.Instance.ShowMessageAsync(this,
                "App will need to be restarted", "This application needs to be restarted for the new theme settings to be applied",
                    MessageDialogStyle.AffirmativeAndNegative);

            if (restartApprovedResult != MessageDialogResult.Affirmative) return;

            // Application restart approved

            // Save new theme settings
            this.SaveAppearanceSettings();

            // Restart application
            this.processHelper.Restart();
        }

        private void ResetAppearanceSettingsToDefault()
        {
            var defaultTheme = Theme.Default;

            this.SelectedStyle = defaultTheme.Style;

            // Convert default theme colors to hex values
            this.ThemeAccentBaseColorHex = defaultTheme.AccentBaseColor.ToHex();
            this.ThemeHighlightColorHex = defaultTheme.HighlightColor.ToHex();
            this.ThemeWindowBorderColorHex = defaultTheme.WindowBorderColor.ToHex();
        }

        private void SaveAppearanceSettings()
        {
            // Convert hex values to colors
            var accentBaseColor = this.ThemeAccentBaseColorHex.HexToColor();
            var highlightColor = this.ThemeHighlightColorHex.HexToColor();
            var windowBorderColor = this.ThemeWindowBorderColorHex.HexToColor();

            // Remove transparency values as they are not supported
            accentBaseColor = accentBaseColor.SetTransparencyFraction(1.0);
            highlightColor = highlightColor.SetTransparencyFraction(1.0);
            windowBorderColor = windowBorderColor.SetTransparencyFraction(1.0);

            var themeStyle = this.SelectedStyle;

            // Build new theme instance
            var newTheme = new Theme
            {
                Style = themeStyle,
                AccentBaseColor = accentBaseColor,
                HighlightColor = highlightColor,
                WindowBorderColor = windowBorderColor
            };

            // Export and save as JSON in settings
            var newThemeJson = Theme.ExportToJson(newTheme);
            Properties.Settings.Default.AppTheme = newThemeJson;
            Properties.Settings.Default.Save();

            // Update settings' current values
            this.currentStyle = this.SelectedStyle;
            this.currentThemeAccentBaseColorHex = this.ThemeAccentBaseColorHex;
            this.currentThemeHighlightColorHex = this.ThemeHighlightColorHex;
            this.currentThemeWindowBorderColorHex = this.ThemeWindowBorderColorHex;

            NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
        }
    }
}