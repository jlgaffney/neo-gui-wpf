using Neo.Gui.Wpf.Controls;
using Neo.UI;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public partial class BulkPayView
    {
        private readonly BulkPayViewModel viewModel;

        internal BulkPayView(AssetDescriptor asset = null)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as BulkPayViewModel;

            this.viewModel?.Load(asset);
        }

        internal TxOutListBoxItem[] GetOutputs()
        {
            return this.viewModel?.GetOutputs();
        }
    }
}