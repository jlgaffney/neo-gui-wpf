using System;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.UI.Core.Controllers.Interfaces;
using Neo.UI.Core.Exceptions;
using Neo.UI.Core.Helpers;
using Neo.UI.Core.Importers;
using Neo.UI.Core.Messaging.Interfaces;
using Neo.UI.Core.Status;

namespace Neo.UI.Core.Controllers.Implementations
{
    internal class LocalBlockchainController :
        BaseBlockchainController,
        IBlockchainController
    {
        #region Private Fields
        private bool initialized;
        private bool disposed;

        private Blockchain blockchain;
        #endregion

        #region Constructor 
        public LocalBlockchainController(
            IMessagePublisher messagePublisher)
                : base(messagePublisher)
        {
        }
        #endregion

        #region IBlockChainController implementation 

        public uint BlockHeight => this.blockchain.Height;

        public event EventHandler<Block> PersistCompleted
        {
            add => Blockchain.PersistCompleted += value;
            remove => Blockchain.PersistCompleted -= value;
        }

        public void Initialize(string blockchainDataDirectoryPath)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IBlockchainController));
            }

            if (this.initialized)
            {
                throw new ObjectAlreadyInitializedException(nameof(IBlockchainController));
            }

            // Setup blockchain
            var levelDBBlockchain = Blockchain.RegisterBlockchain(new LevelDBBlockchain(blockchainDataDirectoryPath));

            Blockchain.PersistCompleted += this.BlockAdded;

            BlockchainImporter.ImportBlocksIfRequired(levelDBBlockchain);

            this.blockchain = levelDBBlockchain;

            this.initialized = true;
        }

        public BlockchainStatus GetStatus()
        {
            if (this.disposed) return null;

            var timeSinceLastBlock = this.GetTimeSinceLastBlock();

            if (timeSinceLastBlock < TimeSpan.Zero)
            {
                timeSinceLastBlock = TimeSpan.Zero;
            }

            bool nextBlockProgressIsIndeterminate;
            double nextBlockProgressFraction;
            if (timeSinceLastBlock > Blockchain.TimePerBlock)
            {
                nextBlockProgressIsIndeterminate = true;
                nextBlockProgressFraction = 1.0;
            }
            else
            {
                nextBlockProgressIsIndeterminate = false;
                nextBlockProgressFraction = (double)timeSinceLastBlock.Seconds / Blockchain.TimePerBlock.Seconds;
            }

            if (nextBlockProgressFraction < 0.0)
            {
                nextBlockProgressFraction = 0.0;
            }
            else if (nextBlockProgressFraction > 1.0)
            {
                nextBlockProgressFraction = 1.0;
            }

            return new BlockchainStatus(this.blockchain.Height, this.blockchain.HeaderHeight,
                nextBlockProgressIsIndeterminate, nextBlockProgressFraction, timeSinceLastBlock);
        }

        public Transaction GetTransaction(UInt256 hash)
        {
            return this.blockchain.GetTransaction(hash);
        }

        public Transaction GetTransaction(UInt256 hash, out int height)
        {
            return this.blockchain.GetTransaction(hash, out height);
        }

        public AccountState GetAccountState(UInt160 scriptHash)
        {
            return this.blockchain.GetAccountState(scriptHash);
        }

        public ContractState GetContractState(UInt160 scriptHash)
        {
            return this.blockchain.GetContract(scriptHash);
        }

        public AssetState GetAssetState(UInt256 assetId)
        {
            return this.blockchain.GetAssetState(assetId);
        }

        public DateTime GetTimeOfBlock(uint blockHeight)
        {
            var unixTimestamp = this.blockchain.GetHeader(blockHeight).Timestamp;

            return TimeHelper.UnixTimestampToDateTime(unixTimestamp);
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.initialized)
                    {
                        Blockchain.PersistCompleted -= this.BlockAdded;

                        this.blockchain.Dispose();
                        this.blockchain = null;
                    }

                    this.disposed = true;
                }
            }
        }

        ~LocalBlockchainController()
        {
            Dispose(false);
        }

        #endregion
    }
}
