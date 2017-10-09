using System;

namespace Neo.UI.Wallets
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

        public void SetSelectedTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= this.TabControl.Items.Count) throw new IndexOutOfRangeException();

            this.TabControl.SelectedIndex = tabIndex;
        }
    }
}