using System;
using Neo.Gui.Cross.Controllers;
using Neo.Ledger;
using Neo.Persistence;

namespace Neo.Gui.Cross.Services
{
    public class BlockchainService : IBlockchainService
    {
        private readonly Func<IApplicationController> applicationControllerFactory;
        private IApplicationController applicationController;

        public BlockchainService(
            Func<IApplicationController> applicationControllerFactory)
        {
            this.applicationControllerFactory = applicationControllerFactory;
        }

        private IApplicationController ApplicationController
        {
            get
            {
                if (applicationController == null)
                {
                    applicationController = applicationControllerFactory();
                }

                return applicationController;
            }
        }

        public uint HeaderHeight => ApplicationController.IsRunning ? Blockchain.Singleton.HeaderHeight : 0;

        public uint Height => ApplicationController.IsRunning ? Blockchain.Singleton.Height : 0;

        public Snapshot GetSnapshot()
        {
            if (!ApplicationController.IsRunning)
            {
                return null;
            }

            return Blockchain.Singleton.GetSnapshot();
        }


        public bool ContainsTransaction(UInt256 transactionId)
        {
            return Blockchain.Singleton.ContainsTransaction(transactionId);
        }

        
        public AssetState GetAssetState(UInt256 assetId)
        {
            using (var snapshot = GetSnapshot())
            {
                return snapshot.Assets.TryGet(assetId);
            }
        }

        public AccountState GetAccountState(UInt160 scriptHash)
        {
            using (var snapshot = GetSnapshot())
            {
                return snapshot.Accounts.TryGet(scriptHash);
            }
        }

        public ContractState GetContractState(UInt160 scriptHash)
        {
            using (var snapshot = GetSnapshot())
            {
                return snapshot.Contracts.TryGet(scriptHash);
            }
        }
    }
}
