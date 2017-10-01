using Neo.Core;
using Neo.Cryptography;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.IO;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI.Views;
using Neo.UI.Views.Updater;
using Neo.UI.ViewModels;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;

using Clipboard = System.Windows.Clipboard;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;

namespace Neo.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly MainViewModel viewModel;

        public MainView(XDocument xdoc = null)
        {
            InitializeComponent();
            if (xdoc == null) return;
            
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var latest = Version.Parse(xdoc.Element("update").Attribute("latest").Value);

            if (version >= latest) return;
            
            this.viewModel = this.DataContext as MainViewModel;

            if (this.viewModel != null)
            {
                this.viewModel.NewVersionXml = xdoc;
                this.viewModel.NewVersionLabel = $"{Strings.DownloadNewVersion}: {latest}";
                this.viewModel.NewVersionVisible = true;
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
    }
}