using System;
using System.Threading.Tasks;

namespace Neo.Gui.Base.Interfaces.Helpers
{
    public interface IDispatchHelper
    {
        Task InvokeOnMainUIThread(Action action);
    }
}