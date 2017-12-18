using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Neo.Gui.Base.MVVM
{
    public class BindableClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
