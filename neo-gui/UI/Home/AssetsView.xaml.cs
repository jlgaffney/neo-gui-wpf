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
        public AssetsView()
        {
            InitializeComponent();
        }

        private void AssetList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.DataContext as AssetsViewModel;

            if (viewModel == null) return;

            if (viewModel.SelectedAsset == null) return;

            var url = string.Format(Settings.Default.Urls.AssetUrl, viewModel.SelectedAsset.Name.Substring(2));

            Process.Start(url);
        }
    }
}