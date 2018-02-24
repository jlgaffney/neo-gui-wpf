using System;
using Neo.Core;
using Neo.Network;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface INodeController : IDisposable
    {
        void Initialize(int localNodePort, int localWSPort);

        NetworkStatus GetStatus();

        void Relay(Transaction transaction);

        void Relay(IInventory inventory);
    }
}
