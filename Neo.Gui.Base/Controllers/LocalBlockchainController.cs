using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.IO;

using Neo.Gui.Base.Helpers;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;

namespace Neo.Gui.Base.Controllers
{
    public class LocalBlockchainController :
        BaseBlockchainController,
        IBlockchainController
    {
        #region Private Fields
        private readonly IMessagePublisher messagePublisher;

        private readonly string blockchainDataDirectoryPath;

        private bool initialized;
        private bool disposed;

        private Blockchain blockchain;

        private DateTime timeOfLastBlock = DateTime.MinValue;
        #endregion

        #region Constructor 
        public LocalBlockchainController(
            IMessagePublisher messagePublisher,
            ISettingsManager settingsManager)
            : base(settingsManager.LocalNodePort, settingsManager.LocalWSPort)
        {
            this.messagePublisher = messagePublisher;

            this.blockchainDataDirectoryPath = settingsManager.BlockchainDataDirectoryPath;
        }
        #endregion

        #region IBlockChainController implementation 

        public uint BlockHeight => this.blockchain.Height;

        public event EventHandler<Block> PersistCompleted
        {
            add => Blockchain.PersistCompleted += value;
            remove => Blockchain.PersistCompleted -= value;
        }

        public override void Initialize()
        {
            base.Initialize();

            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IBlockchainController));
            }

            if (this.initialized)
            {
                throw new Exception(nameof(IBlockchainController) + " has already been initialized!");
            }

            this.InitializeBlockchain();

            this.initialized = true;
        }

        public BlockchainStatus GetStatus()
        {
            if (this.disposed) return null;

            var timeSinceLastBlock = DateTime.UtcNow - this.timeOfLastBlock;

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

            var nodeCount = this.GetRemoteNodeCount();

            return new BlockchainStatus(this.blockchain.Height, this.blockchain.HeaderHeight,
                nextBlockProgressIsIndeterminate, nextBlockProgressFraction, timeSinceLastBlock, nodeCount);
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

        #region Private Methods
        
        private void InitializeBlockchain()
        {
            this.blockchain = new LevelDBBlockchain(blockchainDataDirectoryPath);

            Task.Run(() =>
            {
                ImportBlocksIfRequired(this.blockchain);

                Blockchain.PersistCompleted += this.BlockchainPersistCompleted;
            });
        }
        
        private void BlockchainPersistCompleted(object sender, Block block)
        {
            this.timeOfLastBlock = DateTime.UtcNow;
            this.messagePublisher.Publish(new BlockchainPersistCompletedMessage());
        }

        private static void ImportBlocksIfRequired(Blockchain blockchain)
        {
            if (blockchain == null) return;

            const string accPath = "chain.acc";
            const string accZipPath = accPath + ".zip";

            if (File.Exists(accPath)) // Check if import file exists
            {
                // Import blocks
                bool importCompleted;
                using (var fileStream = new FileStream(accPath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    importCompleted = ImportBlocks(blockchain, fileStream);
                }

                if (importCompleted)
                {
                    File.Delete(accPath);
                }
            }
            else if (File.Exists(accZipPath)) // Check if ZIP import file exists
            {
                // Import blocks
                bool importCompleted;
                using (var fileStream = new FileStream(accZipPath, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                using (var zipStream = zip.GetEntry(accPath).Open())
                {
                    importCompleted = ImportBlocks(blockchain, zipStream);
                }

                if (importCompleted)
                {
                    File.Delete(accZipPath);
                }
            }
        }

        private static bool ImportBlocks(Blockchain blockchain, Stream stream)
        {
            var levelDBBlockchain = blockchain as LevelDBBlockchain;

            if (levelDBBlockchain != null)
            {
                levelDBBlockchain.VerifyBlocks = false;
            }

            using (var reader = new BinaryReader(stream))
            {
                var count = reader.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    var array = reader.ReadBytes(reader.ReadInt32());

                    if (height <= blockchain.Height) continue;

                    var block = array.AsSerializable<Block>();

                    try
                    {
                        blockchain.AddBlock(block);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Blockchain instance has been disposed. This is most likely due to the application exiting

                        return false;
                    }
                }
            }

            if (levelDBBlockchain != null)
            {
                levelDBBlockchain.VerifyBlocks = true;
            }

            return true;
        }

        #endregion
        
        #region IDisposable implementation

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.initialized)
                    {
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