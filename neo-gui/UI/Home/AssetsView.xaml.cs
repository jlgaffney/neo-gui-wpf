using System.Diagnostics;
using System.Windows.Input;
using Neo.Properties;

namespace Neo.UI.Home
{
    /// <summary>
    /// Interaction logic for AssetsView.xaml
    /// </summary>
    public partial class AssetsView
    {
        private AssetsViewModel viewModel;

        public AssetsView()
        {
            InitializeComponent();
        }

        private void AssetList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.viewModel?.SelectedAsset == null) return;
            var url = string.Format(Settings.Default.Urls.AssetUrl, this.viewModel?.SelectedAsset.Name.Substring(2));
            Process.Start(url);
        }
    }
}