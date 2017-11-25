using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Neo.Gui.Base.Interfaces.Helpers;

namespace Neo.UI.Base.Dispatching
{
    public class DispatchHelper : IDispatchHelper
    {
        public Task InvokeOnMainUIThread(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);

            if (action == null || Application.Current == null) return tcs.Task;

            if (Application.Current.Dispatcher.CheckAccess())
            {
                // Already on main UI thread
                action();
                return tcs.Task;
            }

            // Invoke action on main UI thread
            var operation = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);

            return operation.Task;
        }
    }
}