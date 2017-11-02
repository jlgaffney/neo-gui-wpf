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

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (this.viewModel == null) return;

        //    this.viewModel.Load();
        //}

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.viewModel == null) return;

            this.viewModel.Close();
        }
    }
}