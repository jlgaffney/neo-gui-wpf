using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Voting;

namespace Neo.Gui.Cross.Views.Voting
{
    public class VotingView : Window
    {
        public VotingView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<VotingViewModel>();
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
