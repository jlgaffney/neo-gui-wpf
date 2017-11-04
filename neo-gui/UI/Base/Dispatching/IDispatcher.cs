using System;

namespace Neo.UI.Base.Dispatching
{
    public interface IDispatcher
    {
        void DispatchToMainUIThread(Action action);
    }
}