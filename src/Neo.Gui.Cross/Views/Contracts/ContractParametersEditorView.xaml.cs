using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Contracts;

namespace Neo.Gui.Cross.Views.Contracts
{
    public class ContractParametersEditorView : Window
    {
        public ContractParametersEditorView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<ContractParametersEditorViewModel>();
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
