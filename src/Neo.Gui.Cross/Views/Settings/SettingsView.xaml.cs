using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Settings;

namespace Neo.Gui.Cross.Views.Settings
{
    public class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<SettingsViewModel>();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
