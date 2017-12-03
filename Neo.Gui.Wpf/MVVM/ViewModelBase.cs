using Neo.Gui.Base.MVVM;
using Neo.Gui.Wpf.Controls;

namespace Neo.Gui.Wpf.MVVM
{
    public abstract class ViewModelBase : BindableClass
    {
        // TODO Remove all NeoWindow TryClose stuff to a separate window management class, then move this class to Neo.Gui.Base project
        private NeoWindow window;

        public virtual void OnWindowAttached(NeoWindow attachedWindow)
        {
            this.window = attachedWindow;
        }

        public virtual void TryClose()
        {
            this.window?.Close();
        }
    }
}