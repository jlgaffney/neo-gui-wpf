using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Neo.Core;
using Neo.UI.Transactions;

namespace Neo.UI.OldWinForms
{
    [DefaultEvent(nameof(ItemsChanged))]
    internal partial class TxOutListBox : UserControl
    {
        public event EventHandler ItemsChanged;

        public AssetDescriptor Asset { get; set; }

        public int ItemCount => listBox1.Items.Count;

        public IEnumerable<TxOutListBoxItem> Items => listBox1.Items.OfType<TxOutListBoxItem>();

        public bool ReadOnly
        {
            get
            {
                return !panel1.Enabled;
            }
            set
            {
                panel1.Enabled = !value;
            }
        }

        private UInt160 _script_hash = null;
        public UInt160 ScriptHash
        {
            get
            {
                return _script_hash;
            }
            set
            {
                _script_hash = value;
                button3.Enabled = value == null;
            }
        }

        public TxOutListBox()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            if (listBox1.Items.Count > 0)
            {
                listBox1.Items.Clear();
                button2.Enabled = false;
                ItemsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetItems(IEnumerable<TransactionOutput> outputs)
        {
            listBox1.Items.Clear();
            foreach (TransactionOutput output in outputs)
            {
                AssetState asset = Blockchain.Default.GetAssetState(output.AssetId);
                listBox1.Items.Add(new TxOutListBoxItem
                {
                    AssetName = $"{asset.GetName()} ({asset.Owner})",
                    AssetId = output.AssetId,
                    Value = new BigDecimal(output.Value.GetData(), 8),
                    ScriptHash = output.ScriptHash
                });
            }
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = listBox1.SelectedIndices.Count > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var view = new PayToView(this.Asset, this.ScriptHash);
            view.ShowDialog();

            var output = view.GetOutput();

            if (output == null) return;

            listBox1.Items.Add(output);
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (listBox1.SelectedIndices.Count > 0)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndices[0]);
            }
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var view = new BulkPayView(this.Asset);
            view.ShowDialog();

            var outputs = view.GetOutputs();

            if (outputs == null || outputs.Length == 0) return;

            listBox1.Items.AddRange(outputs);
            ItemsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}