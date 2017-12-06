using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;

namespace Neo.Gui.Wpf.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : IDialog<AboutDialogResult>
    {
        public AboutView()
        {
            InitializeComponent();
        }
    }
}
