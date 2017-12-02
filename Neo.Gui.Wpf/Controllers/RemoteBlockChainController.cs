using System;
using System.Collections.Generic;
using Neo.Core;
using Neo.Gui.Base.Controllers;
using Neo.Network;

namespace Neo.Gui.Wpf.Controllers
{
    public class RemoteBlockChainController : IBlockChainController
    {
        public RegisterTransaction GoverningToken => throw new NotImplementedException();

        public RegisterTransaction UtilityToken => throw new NotImplementedException();

        public uint BlockHeight => throw new NotImplementedException();

        public event EventHandler<Block> PersistCompleted
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public void Initialize()
        {
            // Remote nodes are not supported yet
            throw new NotImplementedException();
        }

        public BlockChainStatus GetStatus()
        {
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

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            throw new NotImplementedException();
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
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
