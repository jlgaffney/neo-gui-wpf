using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

using Neo.Properties;
using Neo.UI.Messages;
using Neo.UI.MVVM;
using Neo.UI.ViewModels;

namespace Neo.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly MainViewModel viewModel;

        public MainView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as MainViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null) return;

            this.viewModel.Load();
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.viewModel == null) return;

            this.viewModel.Close();
        }
        
        private void AccountList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.viewModel?.SelectedAccount == null) return;
            var url = string.Format(Settings.Default.Urls.AddressUrl, this.viewModel?.SelectedAccount.Address);
            Process.Start(url);
        }

        private void AssetList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.viewModel?.SelectedAsset == null) return;
            var url = string.Format(Settings.Default.Urls.AssetUrl, this.viewModel?.SelectedAsset.Name.Substring(2));
            Process.Start(url);
        }

        private void TransactionList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.viewModel?.SelectedTransaction == null) return;
            var url = string.Format(Settings.Default.Urls.TransactionUrl, this.viewModel?.SelectedTransaction.Id.Substring(2));
            Process.Start(url);
        }
    }
}