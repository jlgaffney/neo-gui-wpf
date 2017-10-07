using System.Windows;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Base.Controls
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