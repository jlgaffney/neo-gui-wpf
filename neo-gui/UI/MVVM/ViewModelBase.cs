using System.Runtime.CompilerServices;
using System.ComponentModel;

using Neo.UI.Controls;

namespace Neo.UI.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract void OnViewAttached(NeoWindow attachedView);

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}