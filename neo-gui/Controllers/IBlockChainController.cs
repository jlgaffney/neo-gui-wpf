using System;
using Neo.Core;
using Neo.Network;

namespace Neo.Controllers
{
    public interface IBlockChainController : IDisposable
    {
        uint BlockHeight { get; }

        void Setup(bool setupLocalNode = true);

        void Relay(Transaction transaction);

        void Relay(IInventory inventory);
    }
}