using System;
using System.Collections.Generic;
using Neo.Core;
using Neo.Network;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface IBlockchainService : IDisposable
    {
        UInt256 GoverningTokenHash { get; }

        UInt256 UtilityTokenHash { get; }

        event EventHandler BlockAdded;

        uint BlockHeight { get; }

        void Initialize(int localNodePort, int localWSPort, string blockchainDataDirectoryPath);

        BlockchainStatus GetStatus();

        Transaction GetTransaction(UInt256 hash);

        Transaction GetTransaction(UInt256 hash, out int height);

        AccountState GetAccountState(UInt160 scriptHash);

        ContractState GetContractState(UInt160 scriptHash);

        AssetState GetAssetState(UInt256 assetId);

        DateTime GetTimeOfBlock(uint blockHeight);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true);

        Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd);


        NEP5AssetItem GetTotalNEP5Balance(UInt160 nep5ScriptHash, IEnumerable<UInt160> accountScriptHashes);

        IDictionary<UInt160, BigDecimal> GetNEP5Balances(UInt160 nep5ScriptHash, IEnumerable<UInt160> accountScriptHashes);


        void Relay(Transaction transaction);

        void Relay(IInventory inventory);
    }
}