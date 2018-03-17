using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Network;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Internal.Services.Interfaces
{
    public interface IBlockchainService : IDisposable
    {
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

        IDictionary<UInt160, BigInteger> GetNEP5Balances(UInt160 nep5ScriptHash, IEnumerable<UInt160> accountScriptHashes, out byte decimals);

        string GetNEP5TokenName(UInt160 nep5ScriptHash);

        bool GetNEP5TokenNameAndDecimals(UInt160 nep5ScriptHash, out string name, out byte decimals);

        bool Relay(IInventory inventory);
    }
}