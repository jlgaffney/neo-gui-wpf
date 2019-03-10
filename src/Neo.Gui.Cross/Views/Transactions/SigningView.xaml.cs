using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Transactions;

namespace Neo.Gui.Cross.Views.Transactions
{
    public class SigningView : Window
    {
        public SigningView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<SigningViewModel>();
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
