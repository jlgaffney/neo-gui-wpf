using Neo.UniversalWallet.ViewModels;
using System.Windows;

namespace Neo.UniversalWallet
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = new MainWindowViewModel();

            InitializeComponent();
        }
    }
}
