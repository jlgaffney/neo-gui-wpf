using System;
using System.Collections.Generic;
using Neo.Core;
using Neo.Network;
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

        public void Initialize(int localNodePort, int localWSPort, string blockchainDataDirectoryPath)
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

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            throw new NotImplementedException();
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            throw new NotImplementedException();
        }


        public NEP5AssetItem GetTotalNEP5Balance(UInt160 nep5ScriptHash, IEnumerable<UInt160> accountScriptHashes)
        {
            throw new NotImplementedException();
        }

        public IDictionary<UInt160, BigDecimal> GetNEP5Balances(UInt160 nep5ScriptHash, IEnumerable<UInt160> accountScriptHashes)
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

        #region IDisposable implementation

        public void Dispose()
        {
            
        }

        #endregion
    }
}
