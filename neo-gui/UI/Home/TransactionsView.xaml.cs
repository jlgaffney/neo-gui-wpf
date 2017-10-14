using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Neo.Properties;

namespace Neo.UI.Home
{
    /// <summary>
    /// Interaction logic for TransactionsView.xaml
    /// </summary>
    public partial class TransactionsView
    {
        private TransactionsViewModel viewModel;

        public TransactionsView()
        {
            InitializeComponent();
        }

        private void TransactionsView_Loaded(object sender, RoutedEventArgs e)
        {
            this.AttachViewModel();
        }

        private void AttachViewModel()
        {
            // Check if view model has already been attached
            if (this.viewModel != null) return;

            this.viewModel = this.DataContext as TransactionsViewModel;
        }

        private void TransactionList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.viewModel?.SelectedTransaction == null) return;
            var url = string.Format(Settings.Default.Urls.TransactionUrl, this.viewModel?.SelectedTransaction.Id.Substring(2));
            Process.Start(url);
        }
    }
}