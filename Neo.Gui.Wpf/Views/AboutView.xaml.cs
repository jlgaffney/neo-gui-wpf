using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters;

namespace Neo.Gui.Wpf.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : IDialog<AboutLoadParameters>
    {
        public AboutView()
        {
            InitializeComponent();
        }
    }
}
