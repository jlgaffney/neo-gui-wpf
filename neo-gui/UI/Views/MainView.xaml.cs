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
    public partial class MainView : IHandle<CloseWindowMessage>
    {
        private readonly MainViewModel viewModel;

        public MainView(XDocument xdoc = null)
        {
            InitializeComponent();

            EventAggregator.Current.Subscribe(this);

            this.viewModel = this.DataContext as MainViewModel;

            if (xdoc != null)
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var latest = Version.Parse(xdoc.Element("update").Attribute("latest").Value);

                if (version < latest)
                {
                    if (this.viewModel != null)
                    {
                        this.viewModel.NewVersionXml = xdoc;
                        this.viewModel.NewVersionLabel = $"{Strings.DownloadNewVersion}: {latest}";
                        this.viewModel.NewVersionVisible = true;
                    }
                }
            }
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

        public void Handle(CloseWindowMessage message)
        {
            EventAggregator.Current.Unsubscribe(this);

            this.Close();
        }
    }
}