using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Development;

namespace Neo.Gui.Cross.Views.Development
{
    public class TransactionBuilderView : UserControl
    {
        public TransactionBuilderView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<TransactionBuilderViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
