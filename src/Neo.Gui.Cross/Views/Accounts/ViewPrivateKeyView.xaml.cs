using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Accounts;

namespace Neo.Gui.Cross.Views.Accounts
{
    public class ViewPrivateKeyWindow : Window
    {
        public ViewPrivateKeyWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<ViewPrivateKeyViewModel>();
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
