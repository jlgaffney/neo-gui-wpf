using System.ComponentModel;

namespace Neo.UI.Home
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView
    {
        private readonly HomeViewModel viewModel;

        public HomeView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as HomeViewModel;
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            this.viewModel?.Close();
        }
    }
}