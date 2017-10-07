namespace Neo.UI.Transactions
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