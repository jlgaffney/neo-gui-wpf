using System;
using Neo.Core;
using Neo.Network;

namespace Neo.Controllers
{
    public class RemoteBlockChainController : IBlockChainController
    {
        public uint BlockHeight => 0;

        public void Initialize()
        {
            // Remote nodes are not supported yet
            throw new NotImplementedException();
        }

        public void Relay(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public void Relay(IInventory inventory)
        {
            throw new NotImplementedException();
        }

        public BlockChainStatus GetStatus()
        {
            throw new NotImplementedException();
        }

        #region IDisposable implementation
        
        public void Dispose()
        {
            
        }

        #endregion
    }
}
