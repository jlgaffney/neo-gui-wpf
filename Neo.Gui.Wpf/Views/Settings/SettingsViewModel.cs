using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using Neo.Gui.Base.Extensions;
using Neo.Gui.Base.Helpers.Interfaces;
using Neo.Gui.Base.Theming;
using Neo.Gui.Wpf.MVVM;

namespace Neo.Gui.Wpf.Views.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IThemeHelper themeHelper;

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


        public SettingsViewModel(
            IThemeHelper themeHelper)
        {
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
            this.currentThemeStyle = currentTheme.Style;
            this.SelectedThemeStyle = currentTheme.Style;

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
                var transparentBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (string.IsNullOrEmpty(this.ThemeAccentBaseColorHex)) return transparentBrush;

                var accentBaseColor = this.ThemeAccentBaseColorHex.HexToColor();

                accentBaseColor = accentBaseColor.SetTransparencyFraction(1.0);

                return new SolidColorBrush(Color.FromArgb(accentBaseColor.A, accentBaseColor.R, accentBaseColor.G, accentBaseColor.B));
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
                var transparentBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (string.IsNullOrEmpty(this.ThemeHighlightColorHex)) return transparentBrush;

                var highlightColor = this.ThemeHighlightColorHex.HexToColor();

                highlightColor = highlightColor.SetTransparencyFraction(1.0);

                return new SolidColorBrush(Color.FromArgb(highlightColor.A, highlightColor.R, highlightColor.G, highlightColor.B));
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
                var transparentBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if (string.IsNullOrEmpty(this.ThemeWindowBorderColorHex)) return transparentBrush;

                var windowBorderColor = this.ThemeWindowBorderColorHex.HexToColor();

                windowBorderColor = windowBorderColor.SetTransparencyFraction(1.0);

                return new SolidColorBrush(Color.FromArgb(windowBorderColor.A, windowBorderColor.R, windowBorderColor.G, windowBorderColor.B));
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
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void ResetAppearanceSettingsToDefault()
        {
            var defaultTheme = NeoGuiTheme.Default;

            this.SelectedThemeStyle = defaultTheme.Style;

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

            var themeStyle = this.SelectedThemeStyle;

            // Build new theme instance
            var newTheme = new NeoGuiTheme
            {
                Style = themeStyle,
                AccentBaseColor = accentBaseColor,
                HighlightColor = highlightColor,
                WindowBorderColor = windowBorderColor
            };

            // Export and save as JSON in settings
            var newThemeJson = NeoGuiTheme.ExportToJson(newTheme);
            Properties.Settings.Default.AppTheme = newThemeJson;
            Properties.Settings.Default.Save();

            // Update settings' current values
            this.currentThemeStyle = this.SelectedThemeStyle;
            this.currentThemeAccentBaseColorHex = this.ThemeAccentBaseColorHex;
            this.currentThemeHighlightColorHex = this.ThemeHighlightColorHex;
            this.currentThemeWindowBorderColorHex = this.ThemeWindowBorderColorHex;

            NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
        }
    }
}