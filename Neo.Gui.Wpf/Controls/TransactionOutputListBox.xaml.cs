using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

using Neo.Wallets;

using Neo.Gui.Base.Data;
using Neo.Gui.Base.Dialogs.LoadParameters.Transactions;
using Neo.Gui.Base.Dialogs.Results.Transactions;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.MVVM;

namespace Neo.Gui.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for TxOutListBox.xaml
    /// </summary>
    [DefaultEvent(nameof(ItemsChanged))]
    public partial class TransactionOutputListBox
    {
        private static IDialogManager dialogManager;

        public event EventHandler ItemsChanged;

        public TransactionOutputListBox()
        {
            InitializeComponent();

            this.ListBox.DataContext = this;

            this.UpdateRemoveButtonEnabled();
        }

        #region Public properties

        // Dependency Property
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items",
                typeof(ObservableCollection<TransactionOutputItem>), typeof(TransactionOutputListBox),
                new FrameworkPropertyMetadata(null));

        // Dependency Property
        public static readonly DependencyProperty ScriptHashProperty =
            DependencyProperty.Register("ScriptHash",
                typeof(UInt160), typeof(TransactionOutputListBox),
                new FrameworkPropertyMetadata(null,
                    OnScriptHashPropertyChanged,
                    OnCoerceScriptHashProperty));

        // Dependency Property
        public static readonly DependencyProperty AssetProperty =
            DependencyProperty.Register("Asset",
                typeof(AssetDescriptor), typeof(TransactionOutputListBox),
                new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public ObservableCollection<TransactionOutputItem> Items
        {
            get => (ObservableCollection<TransactionOutputItem>)GetValue(ItemsProperty);
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

        #endregion

        #region Public methods

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

        #endregion

        #region Private methods

        private void UpdateRemoveButtonEnabled()
        {
            this.RemoveButton.IsEnabled = this.ListBox.SelectedItem != null;
        }

        private void ListBox_SelectionChanged(object sender, EventArgs e)
        {
            this.UpdateRemoveButtonEnabled();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var result = dialogManager.ShowDialog<PayToDialogResult, PayToLoadParameters>(
                new LoadParameters<PayToLoadParameters>(new PayToLoadParameters(this.Asset, this.ScriptHash)));

            if (result?.Output == null) return;

            // Execute on main UI thread
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.Items.Add(result.Output);

                ItemsChanged?.Invoke(this, EventArgs.Empty);

                this.UpdateRemoveButtonEnabled();
            }));
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            // Execute on main UI thread
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                var itemsToRemove = this.ListBox.SelectedItems.Cast<TransactionOutputItem>();

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
            var result = dialogManager.ShowDialog<BulkPayDialogResult, BulkPayLoadParameters>(
                new LoadParameters<BulkPayLoadParameters>(new BulkPayLoadParameters(this.Asset)));

            if (result.Outputs == null || !result.Outputs.Any()) return;

            // Execute on main UI thread
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                foreach (var output in result.Outputs)
                {
                    this.Items.Add(output);
                }

                ItemsChanged?.Invoke(this, EventArgs.Empty);

                this.UpdateRemoveButtonEnabled();
            }));
        }

        #endregion

        #region Public static methods

        // TODO Find a better way of opening dialogs from within this class than this static setter method
        public static void SetDialogManager(IDialogManager manager)
        {
            dialogManager = manager;
        }

        #endregion

        #region Private static methods

        private static void OnScriptHashPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var listBox = source as TransactionOutputListBox;

            if (listBox == null) return;

            listBox.BulkPayButton.IsEnabled = e.NewValue == null;
        }

        private static object OnCoerceScriptHashProperty(DependencyObject source, object data)
        {
            var listBox = source as TransactionOutputListBox;

            if (listBox == null) return data;

            listBox.BulkPayButton.IsEnabled = data == null;

            return data;
        }

        #endregion
    }
}