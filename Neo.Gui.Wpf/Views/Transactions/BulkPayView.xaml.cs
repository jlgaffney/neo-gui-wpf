using Neo.Gui.Base.Data;
using Neo.Wallets;

namespace Neo.Gui.Wpf.Views.Transactions
{
    public partial class BulkPayView
    {
        private readonly BulkPayViewModel viewModel;

        public BulkPayView(AssetDescriptor asset = null)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as BulkPayViewModel;

            this.viewModel?.Load(asset);
        }

        public TransactionOutputItem[] GetOutputs()
        {
            return this.viewModel?.GetOutputs();
        }
    }
}