using System;
using Neo.Core;
using Neo.Gui.Base.Controllers;
using Neo.Gui.Base.Controllers.Interfaces;
using Neo.Network;

namespace Neo.Gui.Wpf.Controllers
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
