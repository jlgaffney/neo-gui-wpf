using Neo.Gui.Base.Data;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public partial class PayToView
    {
        private readonly PayToViewModel viewModel;

        public PayToView(AssetDescriptor asset = null, UInt160 scriptHash = null)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as PayToViewModel;

            this.viewModel?.Load(asset, scriptHash);
        }

        public TransactionOutputItem GetOutput()
        {
            return this.viewModel?.GetOutput();
        }
    }
}