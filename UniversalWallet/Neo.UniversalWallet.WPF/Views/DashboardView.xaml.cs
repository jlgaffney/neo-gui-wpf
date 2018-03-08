using System.Windows.Controls;
using Neo.UniversalWallet.WPF.ViewModels;

namespace Neo.UniversalWallet.WPF.Views
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
