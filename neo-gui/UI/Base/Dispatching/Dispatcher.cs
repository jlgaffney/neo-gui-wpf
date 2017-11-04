using System;
using System.Windows;
using System.Windows.Threading;

namespace Neo.UI.Base.Dispatching
{
    public class Dispatcher : IDispatcher
    {
        public void DispatchToMainUIThread(Action action)
        {
            if (action == null) return;

            if (Application.Current.Dispatcher.CheckAccess())
            {
                // Already on main UI thread
                action();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
        }
    }
}