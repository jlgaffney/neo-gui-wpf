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
    }
}