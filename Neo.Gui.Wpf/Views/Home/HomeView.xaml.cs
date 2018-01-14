using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.LoadParameters.Home;

namespace Neo.Gui.Wpf.Views.Home
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : IDialog<HomeLoadParameters>
    {
        public HomeView()
        {
            InitializeComponent();
        }
    }
}