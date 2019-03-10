using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Home;

namespace Neo.Gui.Cross.Views.Home
{
    public class WalletView : UserControl
    {
        public WalletView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<WalletViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
