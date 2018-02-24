using System;
using Neo.Core;
using Neo.UI.Core.Data;

namespace Neo.UI.Core.Services.Interfaces
{
    public interface IBlockchainService : IBaseBlockchainService, IDisposable
    {
        event EventHandler BlockAdded;

        uint BlockHeight { get; }

        void Initialize(string blockchainDataDirectoryPath);

        BlockchainStatus GetStatus();

        Transaction GetTransaction(UInt256 hash);

        Transaction GetTransaction(UInt256 hash, out int height);

        AccountState GetAccountState(UInt160 scriptHash);

        ContractState GetContractState(UInt160 scriptHash);

        AssetState GetAssetState(UInt256 assetId);

        DateTime GetTimeOfBlock(uint blockHeight);
    }
}