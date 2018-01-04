using System;
using System.Threading.Tasks;

namespace Neo.Gui.Base.Services.Interfaces
{
    public interface IDispatchService
    {
        Task InvokeOnMainUIThread(Action action);
    }
}