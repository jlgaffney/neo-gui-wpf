using System.Windows.Input;

namespace Neo.UI.Home
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