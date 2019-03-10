using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Assets;

namespace Neo.Gui.Cross.Views.Assets
{
    public class AssetDistributionView : Window
    {
        public AssetDistributionView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<AssetDistributionViewModel>();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
