using System;

namespace Neo.UI.Wallets
{
    public partial class TransferView
    {
        private readonly TransferViewModel viewModel;

        public TransferView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as TransferViewModel;
        }

        private void TxOutListBox_OnItemsChanged(object sender, EventArgs e)
        {
            this.viewModel?.UpdateOkButtonEnabled();
        }
    }
}