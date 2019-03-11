using Neo.Ledger;
using Neo.Persistence;

namespace Neo.Gui.Cross.Services
{
    public interface IBlockchainService
    {
        uint HeaderHeight { get; }

        uint Height { get; }

        Snapshot GetSnapshot();


        bool ContainsTransaction(UInt256 transactionId);


        AssetState GetAssetState(UInt256 assetId);

        AccountState GetAccountState(UInt160 scriptHash);

        ContractState GetContractState(UInt160 scriptHash);
    }
}
