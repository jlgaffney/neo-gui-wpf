using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Neo.Gui.Wpf.Views.Transactions;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for TxOutListBox.xaml
    /// </summary>
    [DefaultEvent(nameof(ItemsChanged))]
    public partial class TxOutListBox
    {
        public event EventHandler ItemsChanged;

        public TxOutListBox()
        {
            InitializeComponent();

            this.ListBox.DataContext = this;

            this.UpdateRemoveButtonEnabled();
        }

        // Dependency Property
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items",
                typeof(ObservableCollection<TxOutListBoxItem>), typeof(TxOutListBox),
                new FrameworkPropertyMetadata(null));

        // Dependency Property
        public static readonly DependencyProperty ScriptHashProperty =
            DependencyProperty.Register("ScriptHash",
                typeof(UInt160), typeof(TxOutListBox),
                new FrameworkPropertyMetadata(null,
                    OnScriptHashPropertyChanged,
                    OnCoerceScriptHashProperty));

        // Dependency Property
        public static readonly DependencyProperty AssetProperty =
            DependencyProperty.Register("Asset",
                typeof(AssetDescriptor), typeof(TxOutListBox),
                new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public ObservableCollection<TxOutListBoxItem> Items
        {
            get => (ObservableCollection<TxOutListBoxItem>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        // .NET Property wrapper
        public UInt160 ScriptHash
        {
            get => (UInt160)GetValue(ScriptHashProperty);
            set => SetValue(ScriptHashProperty, value);
        }

        // .NET Property wrapper
        public AssetDescriptor Asset
        {
            get => (AssetDescriptor) GetValue(AssetProperty);
            set => SetValue(AssetProperty, value);
        }

        public int ItemCount => this.Items.Count;

        public bool ReadOnly
        {
            get => !this.DockPanel.IsEnabled;
            set => this.DockPanel.IsEnabled = !value;
        }

        private void UpdateRemoveButtonEnabled()
        {
            this.RemoveButton.IsEnabled = this.ListBox.SelectedItem != null;
        }

        public void Clear()
        {
            if (this.Items.Count <= 0) return;

            // Execute on main UI thread
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.Items.Clear();

                this.RemoveButton.IsEnabled = false;

                ItemsChanged?.Invoke(this, EventArgs.Empty);

                this.UpdateRemoveButtonEnabled();
            }));
        }

        private void ListBox_SelectionChanged(object sender, EventArgs e)
        {
            this.UpdateRemoveButtonEnabled();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var view = new PayToView(this.Asset, this.ScriptHash);
            view.ShowDialog();

            var output = view.GetOutput();

            if (output == null) return;

            // Execute on main UI thread
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.Items.Add(output);

                ItemsChanged?.Invoke(this, EventArgs.Empty);

                this.UpdateRemoveButtonEnabled();
            }));
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            // Execute on main UI thread
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                var itemsToRemove = this.ListBox.SelectedItems.Cast<TxOutListBoxItem>();

                foreach (var item in itemsToRemove)
                {
                    this.Items.Remove(item);
                }

                ItemsChanged?.Invoke(this, EventArgs.Empty);

                this.UpdateRemoveButtonEnabled();
            }));
        }

        private void BulkPayButton_Click(object sender, EventArgs e)
        {
            var view = new BulkPayView(this.Asset);
            view.ShowDialog();

            var outputs = view.GetOutputs();

            if (outputs == null || outputs.Length == 0) return;

            // Execute on main UI thread
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                foreach (var output in outputs)
                {
                    this.Items.Add(output);
                }

                ItemsChanged?.Invoke(this, EventArgs.Empty);

                this.UpdateRemoveButtonEnabled();
            }));
        }


        private static void OnScriptHashPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var listBox = source as TxOutListBox;

            if (listBox == null) return;

            listBox.BulkPayButton.IsEnabled = e.NewValue == null;
        }

        private static object OnCoerceScriptHashProperty(DependencyObject source, object data)
        {
            var listBox = source as TxOutListBox;

            if (listBox == null) return data;

            listBox.BulkPayButton.IsEnabled = data == null;

            return data;
        }
    }
}