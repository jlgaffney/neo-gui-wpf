using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Settings;

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