using System.Windows;

using Neo.UI.MVVM;

namespace Neo.UI.Controls
{
    public class NeoWindow : Window
    {
        public NeoWindow()
        {
            this.Loaded += (sender, e) =>
            {
                var viewModel = this.DataContext as ViewModelBase;

                if (viewModel != null)
                {
                    viewModel.OnWindowAttached(this);
                }
            };            
        }
    }
}