using System;

namespace Neo.Gui.Wpf.Views.Wallets
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