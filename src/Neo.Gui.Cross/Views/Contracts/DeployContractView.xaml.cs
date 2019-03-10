using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Contracts;

namespace Neo.Gui.Cross.Views.Contracts
{
    public class DeployContractView : Window
    {
        public DeployContractView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<DeployContractViewModel>();
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
