using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Settings;

namespace Neo.Gui.Wpf.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : IDialog<SettingsLoadParameters>
    {
        public SettingsView()
        {
            InitializeComponent();
        }
    }
}