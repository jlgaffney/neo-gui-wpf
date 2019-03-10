using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Home;

namespace Neo.Gui.Cross.Views.Home
{
    public class AssetsView : UserControl
    {
        public AssetsView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<AssetsViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
