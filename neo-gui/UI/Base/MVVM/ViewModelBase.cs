using System.ComponentModel;
using System.Runtime.CompilerServices;
using Neo.UI.Base.Controls;

namespace Neo.UI.Base.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private NeoWindow window;

        public virtual void OnWindowAttached(NeoWindow attachedWindow)
        {
            this.window = attachedWindow;
        }

        public virtual void TryClose()
        {
            this.window?.Close();
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}