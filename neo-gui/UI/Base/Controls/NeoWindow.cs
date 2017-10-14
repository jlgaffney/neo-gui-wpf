using MahApps.Metro.Controls;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Base.Controls
{
    public class NeoWindow : MetroWindow
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