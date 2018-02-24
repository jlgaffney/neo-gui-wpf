using System;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.UI.Core.Data;
using Neo.UI.Core.Helpers;
using Neo.UI.Core.Services.Implementations.Exceptions;
using Neo.UI.Core.Services.Interfaces;

namespace Neo.UI.Core.Services.Implementations
{
    internal class LocalBlockchainService :
        BaseBlockchainService,
        IBlockchainService
    {
        #region Private Fields
        private static readonly TimeSpan OneSecondTimeSpan = new TimeSpan(0, 0, 1);

        private readonly IBlockchainImportService blockchainImportService;

        private bool initialized;
        private bool disposed;

        private Blockchain blockchain;
        private DateTime timeOfLastBlock = DateTime.MinValue;

        private DateTime timeOfLastBlockAddedMessagePublish = DateTime.MinValue;
        #endregion

        #region Constructor 
        public LocalBlockchainService(
            IBlockchainImportService blockchainImportService)
        {
            this.blockchainImportService = blockchainImportService;
        }
        #endregion

        #region IBlockChainController implementation

        public event EventHandler BlockAdded;

        public uint BlockHeight => this.blockchain.Height;

        public void Initialize(string blockchainDataDirectoryPath)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IBlockchainService));
            }

            if (this.initialized)
            {
                throw new ObjectAlreadyInitializedException(nameof(IBlockchainService));
            }

            try
            {
                // Setup blockchain
                var levelDBBlockchain = Blockchain.RegisterBlockchain(new LevelDBBlockchain(blockchainDataDirectoryPath));

                Blockchain.PersistCompleted += this.OnBlockAdded;

                if (this.blockchainImportService.BlocksAreAvailableToImport)
                {
                    Task.Run(() =>
                    {
                        this.blockchainImportService.ImportBlocks(levelDBBlockchain);
                    });
                }

                this.blockchain = levelDBBlockchain;

                this.initialized = true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error initializing BlockchainController {ex.Message}");
            }
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
                        Blockchain.PersistCompleted -= this.OnBlockAdded;

                        this.blockchain.Dispose();
                        this.blockchain = null;
                    }

                    this.disposed = true;
                }
            }
        }

        ~LocalBlockchainService()
        {
            Dispose(false);
        }

        #endregion

        #region Private Methods
        private TimeSpan GetTimeSinceLastBlock()
        {
            return DateTime.UtcNow - this.timeOfLastBlock;
        }

        protected void OnBlockAdded(object sender, Block block)
        {
            var now = DateTime.UtcNow;

            this.timeOfLastBlock = now;

            if (now - this.timeOfLastBlockAddedMessagePublish < OneSecondTimeSpan) return;

            this.BlockAdded?.Invoke(this, EventArgs.Empty);
            
            this.timeOfLastBlockAddedMessagePublish = now;
        }
        #endregion
    }
}
