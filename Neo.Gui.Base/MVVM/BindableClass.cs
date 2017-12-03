using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Neo.Gui.Base.MVVM
{
    // TODO Remove this class when ViewModelBase has been moved to this project
    public class BindableClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
