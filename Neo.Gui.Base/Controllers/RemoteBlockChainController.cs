using System;

using Neo.Core;

using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Gui.Base.Status;

namespace Neo.Gui.Base.Controllers
{
    public class RemoteBlockchainController :
        BaseBlockchainController,
        IBlockchainController
    {
        #region Constructor
        public RemoteBlockchainController(
            IMessagePublisher messagePublisher)
                : base(messagePublisher)
        {
        }
        #endregion

        public uint BlockHeight => throw new NotImplementedException();

        public event EventHandler<Block> PersistCompleted
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public void Initialize(string blockchainDataDirectoryPath)
        {
            // Remote nodes are not supported yet
            throw new NotImplementedException();
        }

        public BlockchainStatus GetStatus()
        {
            throw new NotImplementedException();
        }

        public Transaction GetTransaction(UInt256 hash)
        {
            throw new NotImplementedException();
        }

        public Transaction GetTransaction(UInt256 hash, out int height)
        {
            throw new NotImplementedException();
        }

        public AccountState GetAccountState(UInt160 scriptHash)
        {
            throw new NotImplementedException();
        }

        public ContractState GetContractState(UInt160 scriptHash)
        {
            throw new NotImplementedException();
        }

        public AssetState GetAssetState(UInt256 assetId)
        {
            throw new NotImplementedException();
        }

        public DateTime GetTimeOfBlock(uint blockHeight)
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
