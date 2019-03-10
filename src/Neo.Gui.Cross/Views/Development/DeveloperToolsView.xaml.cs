using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Development;

namespace Neo.Gui.Cross.Views.Development
{
    public class DeveloperToolsView : Window
    {
        public DeveloperToolsView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<DeveloperToolsViewModel>();
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
