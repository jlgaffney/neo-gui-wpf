using System;
using Neo.Gui.Base.Dialogs.Interfaces;
using Neo.Gui.Base.Dialogs.Results;
using Neo.Gui.ViewModels.Wallets;

namespace Neo.Gui.Wpf.Views.Wallets
{
    public partial class TransferView : IDialog<TransferDialogResult>
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