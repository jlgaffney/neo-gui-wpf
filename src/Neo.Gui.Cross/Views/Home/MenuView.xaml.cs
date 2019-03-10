using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Home;

namespace Neo.Gui.Cross.Views.Home
{
    public class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();

            this.DataContext = ViewModelLocator.GetDataContext<MenuViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
