using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results.Home;

namespace Neo.Gui.Wpf.Views.Home
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : IDialog<HomeDialogResult>
    {
        public HomeView()
        {
            InitializeComponent();
        }
    }
}