using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Neo.Core;
using Neo.UI.Transactions;

namespace Neo.UI.Base.Controls.Transactions
{
    /// <summary>
    /// Interaction logic for TxOutListBox.xaml
    /// </summary>
    [DefaultEvent(nameof(ItemsChanged))]
    public partial class TxOutListBox
    {
        public event EventHandler ItemsChanged;

        internal AssetDescriptor Asset { get; set; }

        public int ItemCount => this.ListBox.Items.Count;

        internal IEnumerable<TxOutListBoxItem> Items => this.ListBox.Items.OfType<TxOutListBoxItem>();

        public bool ReadOnly
        {
            get => !this.DockPanel.IsEnabled;
            set => this.DockPanel.IsEnabled = !value;
        }

        private UInt160 _script_hash = null;
        public UInt160 ScriptHash
        {
            get => _script_hash;
            set
            {
                _script_hash = value;
                this.BulkPayButton.IsEnabled = value == null;
            }
        }

        public TxOutListBox()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            if (this.ListBox.Items.Count <= 0) return;

            this.ListBox.Items.Clear();

            this.RemoveButton.IsEnabled = false;

            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetItems(IEnumerable<TransactionOutput> outputs)
        {
            this.ListBox.Items.Clear();

            foreach (var output in outputs)
            {
                var asset = Blockchain.Default.GetAssetState(output.AssetId);

                this.ListBox.Items.Add(new TxOutListBoxItem
                {
                    AssetName = $"{asset.GetName()} ({asset.Owner})",
                    AssetId = output.AssetId,
                    Value = new BigDecimal(output.Value.GetData(), 8),
                    ScriptHash = output.ScriptHash
                });
            }

            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ListBox_SelectionChanged(object sender, EventArgs e)
        {
            this.RemoveButton.IsEnabled = this.ListBox.SelectedItems.Count > 0;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var view = new PayToView(this.Asset, this.ScriptHash);
            view.ShowDialog();

            var output = view.GetOutput();

            if (output == null) return;

            this.ListBox.Items.Add(output);

            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            while (this.ListBox.SelectedItems.Count > 0)
            {
                this.ListBox.Items.Remove(this.ListBox.SelectedItems[0]);
            }

            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void BulkPayButton_Click(object sender, EventArgs e)
        {
            var view = new BulkPayView(this.Asset);
            view.ShowDialog();

            var outputs = view.GetOutputs();

            if (outputs == null || outputs.Length == 0) return;

            foreach (var output in outputs)
            {
                this.ListBox.Items.Add(output);
            }

            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}