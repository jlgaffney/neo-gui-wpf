using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels;

namespace Neo.Gui.Cross.Views
{
    public class ShellWindow : Window
    {
        public ShellWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<ShellWindowViewModel>();
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
