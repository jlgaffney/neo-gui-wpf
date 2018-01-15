using System;
using Neo.Core;
using Neo.Network;
using Neo.UI.Core.Status;

namespace Neo.UI.Core.Controllers.Interfaces
{
    internal interface INetworkController : IDisposable
    {
        void Initialize(int localNodePort, int localWSPort);

        NetworkStatus GetStatus();

        void Relay(Transaction transaction);

        void Relay(IInventory inventory);
    }
}
