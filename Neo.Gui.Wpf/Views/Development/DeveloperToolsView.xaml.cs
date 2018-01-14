using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Development;

namespace Neo.Gui.Wpf.Views.Development
{
    /// <summary>
    /// Interaction logic for DeveloperToolsView.xaml
    /// </summary>
    public partial class DeveloperToolsView : IDialog<DeveloperToolsLoadParameters>
    {
        public DeveloperToolsView()
        {
            InitializeComponent();
        }
    }
}