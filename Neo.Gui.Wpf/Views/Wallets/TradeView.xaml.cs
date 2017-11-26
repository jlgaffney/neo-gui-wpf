using System;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class TradeView
    {
        private readonly TradeViewModel viewModel;

        public TradeView()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as TradeViewModel;
        }

        private void TxOutListBox_OnItemsChanged(object sender, EventArgs e)
        {
            this.viewModel?.UpdateInitiateButtonEnabled();
        }
    }
}