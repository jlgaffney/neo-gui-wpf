using System.Diagnostics;
using System.Windows;
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

        private void AssetsView_Loaded(object sender, RoutedEventArgs e)
        {
            this.AttachViewModel();
        }

        private void AttachViewModel()
        {
            // Check if view model has already been attached
            if (this.viewModel != null) return;

            this.viewModel = this.DataContext as AssetsViewModel;
        }

        private void AssetList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.viewModel?.SelectedAsset == null) return;
            var url = string.Format(Settings.Default.Urls.AssetUrl, this.viewModel?.SelectedAsset.Name.Substring(2));
            Process.Start(url);
        }
    }
}