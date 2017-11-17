using System.ComponentModel;

namespace Neo.UI.Home
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView
    {
        public HomeView()
        {
            InitializeComponent();
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            // TODO - Issue #42 [AboimPinto] - the closing of the windows event should go to ViewModel through the IUnloadable interface.
            //var viewModel = this.DataContext as HomeViewModel;
            //viewModel?.Close();
        }
    }
}