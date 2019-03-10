using System;
using ReactiveUI;

namespace Neo.Gui.Cross.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject
    {
        public event EventHandler Close;

        /// <summary>
        /// Fires <see cref="Close" /> event, which is handled by
        /// another service to manage closing of associated views.
        /// </summary>
        protected virtual void OnClose()
        {
            Close?.Invoke(this, EventArgs.Empty);
        }
    }
}
