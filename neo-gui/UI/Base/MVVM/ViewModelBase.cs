using System.Runtime.CompilerServices;
using System.ComponentModel;
using Neo.UI.Controls;

namespace Neo.UI.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private NeoWindow window;

        public virtual void OnWindowAttached(NeoWindow window)
        {
            this.window = window;
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