using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Contracts;

namespace Neo.Gui.Cross.Views.Contracts
{
    public class InvokeContractView : Window
    {
        public InvokeContractView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<InvokeContractViewModel>();
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
