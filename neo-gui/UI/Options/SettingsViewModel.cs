using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro;
using Neo.Properties;
using Neo.UI.Base.Extensions;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Options
{
    public class SettingsViewModel : ViewModelBase
    {
        private string currentNEP5ContractsList;
        private string nep5ContractsList;

        private NeoTheme currentAppTheme;
        private NeoTheme selectedAppTheme;

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
            var appTheme = (NeoTheme)Enum.ToObject(typeof(NeoTheme), Settings.Default.AppTheme);

            this.currentAppTheme = appTheme;
            this.SelectedAppTheme = appTheme;
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

        public ICommand ApplyNEP5SettingsCommand => new RelayCommand(this.ApplyNEP5Settings);

        #endregion NEP-5 Properties & Commands

        #region Appearance Properties

        public NeoTheme[] AppThemes => Enum.GetValues(typeof(NeoTheme)).Cast<NeoTheme>().ToArray();

        public NeoTheme SelectedAppTheme
        {
            get => this.selectedAppTheme;
            set
            {
                if (this.selectedAppTheme == value) return;

                this.selectedAppTheme = value;

                NotifyPropertyChanged();

                // Update dependent property
                NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
            }
        }

        public bool AppearanceSettingsChanged => this.currentAppTheme != this.SelectedAppTheme;

        public ICommand ApplyAppearanceSettingsCommand => new RelayCommand(this.ApplyAppearanceSettings);

        #endregion Appearance Properties

        public ICommand OkCommand => new RelayCommand(this.Ok);

        public ICommand CancelCommand => new RelayCommand(this.Cancel);

        public void Ok()
        {
            if (this.NEP5SettingsChanged || this.AppearanceSettingsChanged)
            {
                // TODO Show message warning user their settings
                // changes will be discarded if they continue
            }

            this.TryClose();
        }

        public void Cancel()
        {
            this.TryClose();
        }

        public void ApplyNEP5Settings()
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

        public void ApplyAppearanceSettings()
        {
            Settings.Default.AppTheme = (int) this.SelectedAppTheme;

            Settings.Default.Save();

            // If theme has changed apply new application theme
            if (this.SelectedAppTheme != this.currentAppTheme)
            {
                App.SetTheme(this.SelectedAppTheme);
            }

            // Update settings' current values
            this.currentAppTheme = this.SelectedAppTheme;
            NotifyPropertyChanged(nameof(this.AppearanceSettingsChanged));
        }
    }
}