using MahApps.Metro.Controls;

namespace Neo.Gui.Wpf.Controls
{
    public class NeoWindow : MetroWindow
    {
        public NeoWindow()
        {
            this.SetResourceReference(StyleProperty, "DefaultWindowStyle");
        }
    }
}