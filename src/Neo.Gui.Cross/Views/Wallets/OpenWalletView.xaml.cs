using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Wallets;

namespace Neo.Gui.Cross.Views.Wallets
{
    public class OpenWalletView : Window
    {
        public OpenWalletView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<OpenWalletViewModel>();
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
