using System;
using Neo.Core;

namespace Neo.UI.Assets
{
    public partial class AssetDistributionView
    {
        private readonly AssetDistributionViewModel viewModel;

        public AssetDistributionView(AssetState asset = null)
        {
            InitializeComponent();

            this.viewModel = this.DataContext as AssetDistributionViewModel;

            this.viewModel?.SetAsset(asset);
        }

        public IssueTransaction GetTransaction()
        {
            return this.viewModel?.GetTransaction();
        }

        private void TxOutListBox_OnItemsChanged(object sender, EventArgs e)
        {
            this.viewModel?.UpdateConfirmButtonEnabled();
        }
    }
}