using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Voting;

namespace Neo.Gui.Cross.Views.Voting
{
    public class ElectionView : Window
    {
        public ElectionView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<ElectionViewModel>();
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
