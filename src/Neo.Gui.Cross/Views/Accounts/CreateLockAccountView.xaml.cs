using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Accounts;

namespace Neo.Gui.Cross.Views.Accounts
{
    public class CreateLockAccountView : Window
    {
        public CreateLockAccountView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<CreateLockAccountViewModel>();
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
