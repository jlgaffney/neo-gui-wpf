using System.Diagnostics;
using System.Windows.Input;
using Neo.Properties;

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
            var viewModel = this.DataContext as TransactionsViewModel;

            if (viewModel == null) return;

            if (viewModel.SelectedTransaction == null) return;

            if (string.IsNullOrEmpty(viewModel.SelectedTransaction.Id)) return;

            var url = string.Format(Settings.Default.Urls.TransactionUrl, viewModel.SelectedTransaction.Id.Substring(2));

            Process.Start(url);
        }
    }
}