using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Home;

namespace Neo.Gui.Cross.Views.Home
{
    public class AccountsView : UserControl
    {
        public AccountsView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<AccountsViewModel>();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
