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
    }
}
