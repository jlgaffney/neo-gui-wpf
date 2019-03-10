using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Development;

namespace Neo.Gui.Cross.Views.Development
{
    public class ContractParametersView : UserControl
    {
        public ContractParametersView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<ContractParametersViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
