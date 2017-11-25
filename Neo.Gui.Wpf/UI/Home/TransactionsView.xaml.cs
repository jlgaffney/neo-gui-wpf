using System.Windows.Input;

namespace Neo.UI.Home
{
    /// <summary>
    /// Interaction logic for TransactionsView.xaml
    /// </summary>
    public partial class TransactionsView
    {
        public TransactionsView()
        {
            InitializeComponent();
        }

        private void TransactionList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as TransactionsViewModel)?.ViewSelectedTransactionDetailsCommand.Execute(null);
        }
    }
}