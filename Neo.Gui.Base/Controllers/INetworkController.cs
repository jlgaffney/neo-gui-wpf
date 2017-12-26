using System;

using Neo.Core;
using Neo.Network;

using Neo.Gui.Base.Status;

namespace Neo.Gui.Base.Controllers
{
    internal interface INetworkController : IDisposable
    {
        void Initialize(int localNodePort, int localWSPort);

        NetworkStatus GetStatus();

        void Relay(Transaction transaction);

        void Relay(IInventory inventory);
    }
}
