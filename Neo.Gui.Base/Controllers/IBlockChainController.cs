using System;
using System.Collections.Generic;
using Neo.Core;
using Neo.Network;

namespace Neo.Gui.Base.Controllers
{
    public interface IBlockchainController : IDisposable
    {
        RegisterTransaction GoverningToken { get; }

        RegisterTransaction UtilityToken { get; }
        
        uint BlockHeight { get; }

        event EventHandler<Block> PersistCompleted;

        void Initialize();

        BlockchainStatus GetStatus();

        void Relay(Transaction transaction);

        void Relay(IInventory inventory);

        Transaction GetTransaction(UInt256 hash);

        Transaction GetTransaction(UInt256 hash, out int height);

        AccountState GetAccountState(UInt160 scriptHash);

        ContractState GetContractState(UInt160 scriptHash);

        AssetState GetAssetState(UInt256 assetId);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd);
    }
}