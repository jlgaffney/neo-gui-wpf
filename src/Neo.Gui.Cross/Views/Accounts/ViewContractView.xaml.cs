using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Accounts;

namespace Neo.Gui.Cross.Views.Accounts
{
    public class ViewContractView : Window
    {
        public ViewContractView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<ViewContractViewModel>();
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
