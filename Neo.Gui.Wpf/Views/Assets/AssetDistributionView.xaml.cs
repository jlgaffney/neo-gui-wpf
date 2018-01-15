using System;
using Neo.Core;
using Neo.Gui.Dialogs.Interfaces;
using Neo.Gui.Dialogs.LoadParameters.Assets;
using Neo.Gui.ViewModels.Assets;

namespace Neo.Gui.Wpf.Views.Assets
{
    public partial class AssetDistributionView : IDialog<AssetDistributionLoadParameters>
    {
        private readonly AssetDistributionViewModel viewModel;

        public AssetDistributionView(AssetState asset = null)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as AssetDistributionViewModel;

            this.viewModel?.SetAsset(asset);
        }

        private void TxOutListBox_OnItemsChanged(object sender, EventArgs e)
        {
            this.viewModel?.UpdateConfirmButtonEnabled();
        }
    }
}