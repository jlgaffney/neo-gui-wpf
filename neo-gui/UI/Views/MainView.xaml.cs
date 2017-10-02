using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
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
    }
}