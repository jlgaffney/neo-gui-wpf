using Neo.Gui.Wpf.Controls;
using Neo.UI;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public partial class PayToView
    {
        private readonly PayToViewModel viewModel;

        internal PayToView(AssetDescriptor asset = null, UInt160 scriptHash = null)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as PayToViewModel;

            this.viewModel?.Load(asset, scriptHash);
        }

        internal TxOutListBoxItem GetOutput()
        {
            return this.viewModel?.GetOutput();
        }
    }
}