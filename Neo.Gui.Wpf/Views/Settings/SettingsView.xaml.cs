using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.Base.Dialogs.Results.Settings;

namespace Neo.Gui.Wpf.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : IDialog<SettingsDialogResult>
    {
        public SettingsView()
        {
            InitializeComponent();
        }
    }
}