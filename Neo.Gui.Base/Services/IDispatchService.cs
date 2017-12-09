using System;
using System.Threading.Tasks;

namespace Neo.Gui.Base.Services
{
    public interface IDispatchService
    {
        Task InvokeOnMainUIThread(Action action);
    }
}