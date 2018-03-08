using System.Windows;
using Neo.UniversalWallet.WPF.ViewModels;

namespace Neo.UniversalWallet.WPF
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
