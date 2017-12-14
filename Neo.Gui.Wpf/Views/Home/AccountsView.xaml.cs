using System.Windows.Input;
using Neo.Gui.ViewModels.Home;

namespace Neo.Gui.Wpf.Views.Home
{
    /// <summary>
    /// Interaction logic for AccountsView.xaml
    /// </summary>
    public partial class AccountsView
    {
        public AccountsView()
        {
            InitializeComponent();
        }
        
        private void AccountList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as AccountsViewModel)?.ViewSelectedAccountDetailsCommand.Execute(null);
        }
    }
}