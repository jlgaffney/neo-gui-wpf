using System.Windows.Controls;
using Neo.UniversalWallet.ViewModels;

namespace Neo.UniversalWallet.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Page
    {
        public DashboardView()
        {
            this.DataContext = new DashboardViewModel();
            InitializeComponent();
        }
    }
}
