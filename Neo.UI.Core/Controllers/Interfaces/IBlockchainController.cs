using System;
using Neo.Core;
using Neo.UI.Core.Status;

namespace Neo.UI.Core.Controllers.Interfaces
{
    internal interface IBlockchainController : IBaseBlockchainController, IDisposable
    {
        uint BlockHeight { get; }

        event EventHandler<Block> PersistCompleted;

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