using System;
using Neo.Core;

namespace Neo.Controllers
{
    public interface IBlockChainController : IDisposable
    {
        void Setup(bool setupLocalNode = true);

        void Relay(Transaction transaction);
    }
}