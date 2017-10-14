using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Neo.UI.Base.MVVM;

namespace Neo.UI.Base.Controls
{
    public class NeoWindow : MetroWindow
    {
        public NeoWindow()
        {
            this.BorderThickness = new Thickness(1.0);
            this.BorderBrush = (Brush) new BrushConverter().ConvertFromString("#64B563");

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