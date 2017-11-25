using System;
using System.Threading.Tasks;

namespace Neo.Gui.Base.Helpers.Interfaces
{
    public interface IDispatchHelper
    {
        Task InvokeOnMainUIThread(Action action);
    }
}