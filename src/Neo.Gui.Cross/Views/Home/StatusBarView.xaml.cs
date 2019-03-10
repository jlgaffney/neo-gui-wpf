using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Home;

namespace Neo.Gui.Cross.Views.Home
{
    public class StatusBarView : UserControl
    {
        public StatusBarView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<StatusBarViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
