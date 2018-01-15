using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Neo.Gui.Dialogs;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Settings;
using Neo.Gui.Base.Managers.Interfaces;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Extensions;
using Neo.UI.Core.Managers.Interfaces;
using Neo.UI.Core.Theming;

namespace Neo.Gui.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase, IDialogViewModel<SettingsLoadParameters>
    {
        private readonly IDialogManager dialogManager;
        private readonly IWalletController walletController;
        private readonly IProcessManager processManager;
        private readonly ISettingsManager settingsManager;
        private readonly IThemeManager themeManager;

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
            IDialogManager dialogManager,
            IWalletController walletController,
            IProcessManager processManager,
            ISettingsManager settingsManager,
            IThemeManager themeManager)
        {
            this.dialogManager = dialogManager;
            this.walletController = walletController;
            this.processManager = processManager;
            this.settingsManager = settingsManager;
            this.themeManager = themeManager;

            this.LoadSettings();
        }

        private void LoadSettings()
        {
            this.LoadNEP5Settings();
            this.LoadAppearanceSettings();

            RaisePropertyChanged(nameof(this.NEP5SettingsChanged));
            RaisePropertyChanged(nameof(this.AppearanceSettingsChanged));
        }

        private void LoadNEP5Settings()
        {
            var nep5WatchScriptHashes = this.walletController.GetNEP5WatchScriptHashes();

            var nep5ContractsLines = nep5WatchScriptHashes.Select(scriptHash => scriptHash.ToString()).ToArray();

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
            var currentTheme = this.themeManager.CurrentTheme;

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

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.NEP5SettingsChanged));
            }
        }

        public bool NEP5SettingsChanged => this.currentNEP5ContractsList != this.NEP5ContractsList;

        public RelayCommand SaveNEP5SettingsCommand => new RelayCommand(this.SaveNEP5Settings);

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

                RaisePropertyChanged();

                // Update dependent property
                RaisePropertyChanged(nameof(this.AppearanceSettingsChanged));
            }
        }

        public string ThemeAccentBaseColorHex
        {
            get => this.themeAccentBaseColorHex;
            set
            {
                if (this.themeAccentBaseColorHex == value) return;

                this.themeAccentBaseColorHex = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.AppearanceSettingsChanged));

                RaisePropertyChanged(nameof(this.ThemeAccentBaseColor));
            }
        }

        public Color ThemeAccentBaseColor
        {
            get
            {
                if (string.IsNullOrEmpty(this.ThemeAccentBaseColorHex)) return Color.Transparent;

                var accentBaseColor = this.ThemeAccentBaseColorHex.HexToColor();

                if (accentBaseColor != Color.Transparent)
                {
                    accentBaseColor = accentBaseColor.SetTransparencyFraction(1.0);
                }

                return accentBaseColor;
            }
        }

        public string ThemeHighlightColorHex
        {
            get => this.themeHighlightColorHex;
            set
            {
                if (this.themeHighlightColorHex == value) return;

                this.themeHighlightColorHex = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.AppearanceSettingsChanged));

                RaisePropertyChanged(nameof(this.ThemeHighlightColor));
            }
        }

        public Color ThemeHighlightColor
        {
            get
            {
                if (string.IsNullOrEmpty(this.ThemeHighlightColorHex)) return Color.Transparent;

                var highlightColor = this.ThemeHighlightColorHex.HexToColor();

                if (highlightColor != Color.Transparent)
                {
                    highlightColor = highlightColor.SetTransparencyFraction(1.0);
                }

                return highlightColor;
            }
        }

        public string ThemeWindowBorderColorHex
        {
            get => this.themeWindowBorderColorHex;
            set
            {
                if (this.themeWindowBorderColorHex == value) return;

                this.themeWindowBorderColorHex = value;

                RaisePropertyChanged();

                // Update dependent properties
                RaisePropertyChanged(nameof(this.AppearanceSettingsChanged));

                RaisePropertyChanged(nameof(this.ThemeWindowBorderColor));
            }
        }

        public Color ThemeWindowBorderColor
        {
            get
            {
                if (string.IsNullOrEmpty(this.ThemeWindowBorderColorHex)) return Color.Transparent;

                var windowBorderColor = this.ThemeWindowBorderColorHex.HexToColor();

                if (windowBorderColor != Color.Transparent)
                {
                    windowBorderColor = windowBorderColor.SetTransparencyFraction(1.0);
                }

                return windowBorderColor;
            }
        }

        public bool AppearanceSettingsChanged =>
            this.currentStyle != this.SelectedStyle ||
            this.currentThemeAccentBaseColorHex != this.ThemeAccentBaseColorHex ||
            this.currentThemeHighlightColorHex != this.ThemeHighlightColorHex ||
            this.currentThemeWindowBorderColorHex != this.ThemeWindowBorderColorHex;

        public RelayCommand ResetAppearanceSettingsToDefaultCommand => new RelayCommand(this.ResetAppearanceSettingsToDefault);

        public RelayCommand SaveAppearanceSettingsCommand => new RelayCommand(this.SaveAppearanceSettings);

        public RelayCommand ApplyAppearanceSettingsCommand => new RelayCommand(this.ApplyAppearanceSettings);

        #endregion Appearance Properties

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        #region IDialogViewModel implementation 
        public event EventHandler Close;

        public void OnDialogLoad(SettingsLoadParameters parameters)
        {
        }
        #endregion

        private void Ok()
        {
            if (this.NEP5SettingsChanged || this.AppearanceSettingsChanged)
            {
                // TODO Show message warning user their settings
                // changes will be discarded if they continue
            }

            this.Close(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            this.Close(this, EventArgs.Empty);
        }

        private void SaveNEP5Settings()
        {
            var nep5WatchScriptHashesHexLines =  string.IsNullOrEmpty(this.NEP5ContractsList)
                ? new string[0] : this.NEP5ContractsList.ToLines();

            var validNEP5WatchScriptHashesHex = new List<string>();

            foreach (var nep5WatchScriptHashHex in nep5WatchScriptHashesHexLines)
            {
                if (string.IsNullOrWhiteSpace(nep5WatchScriptHashHex)) continue;
                
                if (!UInt160.TryParse(nep5WatchScriptHashHex, out var _)) continue;

                validNEP5WatchScriptHashesHex.Add(nep5WatchScriptHashHex);
            }

            this.settingsManager.NEP5WatchScriptHashes = validNEP5WatchScriptHashesHex.ToArray();
            this.settingsManager.Save();

            this.walletController.SetNEP5WatchScriptHashes(validNEP5WatchScriptHashesHex);
            
            // Update settings' current values
            this.currentNEP5ContractsList = this.NEP5ContractsList;
            RaisePropertyChanged(nameof(this.NEP5SettingsChanged));
        }

        private  void ApplyAppearanceSettings()
        {
            var restartApprovedResult = this.dialogManager.ShowMessageDialog("App will need to be restarted",
                "This application needs to be restarted for the new theme settings to be applied",
                    MessageDialogType.YesNo, MessageDialogResult.No);

            if (restartApprovedResult != MessageDialogResult.Yes) return;

            // Application restart approved

            // Save new theme settings
            this.SaveAppearanceSettings();

            // Restart application
            this.processManager.Restart();
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
            // TODO Validate color values

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
            this.settingsManager.AppThemeJson = newThemeJson;
            this.settingsManager.Save();

            // Update settings' current values
            this.currentStyle = this.SelectedStyle;
            this.currentThemeAccentBaseColorHex = this.ThemeAccentBaseColorHex;
            this.currentThemeHighlightColorHex = this.ThemeHighlightColorHex;
            this.currentThemeWindowBorderColorHex = this.ThemeWindowBorderColorHex;

            RaisePropertyChanged(nameof(this.AppearanceSettingsChanged));
        }
    }
}