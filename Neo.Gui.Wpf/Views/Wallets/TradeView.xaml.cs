using System;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Wallets;
using Neo.Gui.ViewModels.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class TradeView : IDialog<TradeLoadParameters>
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