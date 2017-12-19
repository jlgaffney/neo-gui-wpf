using System.Windows.Input;
using Neo.Gui.ViewModels.Home;

namespace Neo.Gui.Wpf.Views.Home
{
    /// <summary>
    /// Interaction logic for AssetsView.xaml
    /// </summary>
    public partial class AssetsView
    {
        public AssetsView()
        {
            InitializeComponent();
        }

        private void AssetList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as AssetsViewModel)?.ViewSelectedAssetDetailsCommand.Execute(null);
        }
    }
}