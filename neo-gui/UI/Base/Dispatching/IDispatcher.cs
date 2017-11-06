using System;
using System.Threading.Tasks;

namespace Neo.UI.Base.Dispatching
{
    public interface IDispatcher
    {
        Task InvokeOnMainUIThread(Action action);
    }
}