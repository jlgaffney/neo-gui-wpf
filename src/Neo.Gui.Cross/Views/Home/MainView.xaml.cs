using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Home;

namespace Neo.Gui.Cross.Views.Home
{
    public class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            this.DataContext = ViewModelLocator.GetDataContext<MainViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
