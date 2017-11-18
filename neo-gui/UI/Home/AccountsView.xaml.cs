using System.Windows.Input;

namespace Neo.UI.Home
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