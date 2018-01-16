using System;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Assets;

namespace Neo.Gui.Wpf.Views.Assets
{
    public partial class AssetDistributionView : IDialog<AssetDistributionLoadParameters>
    {
        public AssetDistributionView()
        {
            InitializeComponent();
        }

        private void TxOutListBox_OnItemsChanged(object sender, EventArgs e)
        {
            // TODO #Issue 145 [AboimPinto]: need to find in the ViewModel a way to enable the confirm buttom

            var viewModel = this.DataContext as AssetDistributionViewModel;
            viewModel.UpdateConfirmButtonEnabled();
        }
    }
}