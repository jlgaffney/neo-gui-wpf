using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Neo.UI.Base.Dispatching
{
    public class Dispatcher : IDispatcher
    {
        public Task InvokeOnMainUIThread(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);

            if (action == null) return tcs.Task;

            if (Application.Current.Dispatcher.CheckAccess())
            {
                // Already on main UI thread
                action();
                return tcs.Task;
            }
            else
            {
                var operation = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);

                return operation.Task;
            }
        }
    }
}