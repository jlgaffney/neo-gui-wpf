using System;
using Neo.Core;
using Neo.UI.Core.Data;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Services.Implementations
{
    internal class RemoteBlockchainService :
        BaseBlockchainService,
        IBlockchainService
    {
        public uint BlockHeight => throw new NotImplementedException();

        public event EventHandler BlockAdded;

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
