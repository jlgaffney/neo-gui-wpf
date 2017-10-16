using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Neo.Properties;
using Neo.UI.Base.Extensions;
using Neo.UI.Base.Helpers;
using Neo.UI.Base.MVVM;
using Neo.UI.Base.Themes;

namespace Neo.UI.Options
{
    public class SettingsViewModel : ViewModelBase
    {
        private string currentNEP5ContractsList;
        private string nep5ContractsList;

        private ThemeStyle currentThemeStyle;
        private ThemeStyle selectedThemeStyle;

        private string currentThemeAccentBaseColorHex;
        private string themeAccentBaseColorHex;

        private string currentThemeHighlightColorHex;
        private string themeHighlightColorHex;

        private string currentThemeWindowBorderColorHex;
        private string themeWindowBorderColorHex;


        public SettingsViewModel()
        {
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
            var nep5ContractsLines = Settings.Default.NEP5Watched.OfType<string>().ToArray();

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
            var currentTheme = NeoTheme.Current;

            // Set theme style
            this.currentThemeStyle = currentTheme.Style;
            this.SelectedThemeStyle = currentTheme.Style;

            // Set color values, without transparency as these colors do not require transparency
            var accentBaseColorHex = ColorHelper.ColorToHex(currentTheme.AccentBaseColor);
            this.currentThemeAccentBaseColorHex = accentBaseColorHex;
            this.ThemeAccentBaseColorHex = accentBaseColorHex;

            var highlightColorHex = ColorHelper.ColorToHex(currentTheme.HighlightColor);
            this.currentThemeHighlightColorHex = highlightColorHex;
            this.ThemeHighlightColorHex = highlightColorHex;

            var windowBorderColorHex = ColorHelper.ColorToHex(currentTheme.WindowBorderColor);
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

        public ThemeStyle[] ThemeStyles => Enum.GetValues(typeof(ThemeStyle)).Cast<ThemeStyle>().ToArray();

        public ThemeStyle SelectedThemeStyle
        {
            get => this.selectedThemeStyle;
            set
            {
                if (this.selectedThemeStyle == value) return;

                this.selectedThemeStyle = value;

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

                // Update dependent property
                NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
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

                // Update dependent property
                NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
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

                // Update dependent property
                NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
            }
        }

        public bool AppearanceSettingsChanged =>
            this.currentThemeStyle != this.SelectedThemeStyle ||
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

            Settings.Default.NEP5Watched.Clear();
            Settings.Default.NEP5Watched.AddRange(nep5ContractsLines.Where(p =>
                !string.IsNullOrWhiteSpace(p) && UInt160.TryParse(p, out _)).ToArray());

            Settings.Default.Save();

            
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
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void ResetAppearanceSettingsToDefault()
        {
            this.SelectedThemeStyle = ThemeHelper.DefaultTheme.Style;

            // Convert default theme colors to hex values
            this.ThemeAccentBaseColorHex = ColorHelper.ColorToHex(ThemeHelper.DefaultTheme.AccentBaseColor);
            this.ThemeHighlightColorHex = ColorHelper.ColorToHex(ThemeHelper.DefaultTheme.HighlightColor);
            this.ThemeWindowBorderColorHex = ColorHelper.ColorToHex(ThemeHelper.DefaultTheme.WindowBorderColor);
        }

        private void SaveAppearanceSettings()
        {
            // Convert hex values to colors
            var accentBaseColor = ColorHelper.HexToColor(this.ThemeAccentBaseColorHex);
            var highlightColor = ColorHelper.HexToColor(this.ThemeHighlightColorHex);
            var windowBorderColor = ColorHelper.HexToColor(this.ThemeWindowBorderColorHex);

            // Remove unnecessary transparency values
            accentBaseColor.A = byte.MaxValue;
            highlightColor.A = byte.MaxValue;
            windowBorderColor.A = byte.MaxValue;

            // Build new theme instance
            var newTheme = new NeoTheme
            {
                Style = this.SelectedThemeStyle,
                AccentBaseColor = accentBaseColor,
                HighlightColor = highlightColor,
                WindowBorderColor = windowBorderColor
            };

            // Export and save as JSON in settings
            var newThemeJson = NeoTheme.Export(newTheme);
            Settings.Default.AppTheme = newThemeJson;
            Settings.Default.Save();

            // Update settings' current values
            this.currentThemeStyle = this.SelectedThemeStyle;
            this.currentThemeAccentBaseColorHex = this.ThemeAccentBaseColorHex;
            this.currentThemeHighlightColorHex = this.ThemeHighlightColorHex;
            this.currentThemeWindowBorderColorHex = this.ThemeWindowBorderColorHex;

            NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
        }
    }
}