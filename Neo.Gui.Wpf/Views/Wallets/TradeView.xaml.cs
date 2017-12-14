using System;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.ViewModels.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class TradeView : IDialog<TradeDialogResult>
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