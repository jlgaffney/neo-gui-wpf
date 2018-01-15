using System;
using System.Threading.Tasks;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface IDispatchService
    {
        Task InvokeOnMainUIThread(Action action);
    }
}