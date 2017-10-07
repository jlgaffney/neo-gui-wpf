namespace Neo.UI.Transactions
{
    public partial class SignatureView
    {
        private readonly SignatureViewModel viewModel;

        internal SignatureView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as SignatureViewModel;

            //this.viewModel?.Load(asset);
        }

        /*internal TxOutListBoxItem[] GetOutputs()
        {
            return this.viewModel?.GetOutputs();
        }*/
    }
}