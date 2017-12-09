using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Gui.Base.Managers;
using Neo.Gui.Base.Messages;
using Neo.Gui.Base.Messaging.Interfaces;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.IO;
using Neo.Network;

namespace Neo.Gui.Base.Controllers
{
    public class LocalBlockchainController : IBlockchainController
    {
        private const string PeerStatePath = "peers.dat";

        #region Private Fields
        private readonly IMessagePublisher messagePublisher;
        private readonly ISettingsManager settingsManager;
        
        private bool disposed = false;

        private Blockchain blockChain;

        private LocalNode localNode;

        private DateTime timeOfLastBlock = DateTime.MinValue;
       #endregion

        #region Constructor 
        public LocalBlockchainController(
            IMessagePublisher messagePublisher,
            ISettingsManager settingsManager)
        {
            this.messagePublisher = messagePublisher;
            this.settingsManager = settingsManager;
        }
        #endregion

        #region IBlockChainController implementation 

        public RegisterTransaction GoverningToken => Blockchain.GoverningToken;

        public RegisterTransaction UtilityToken => Blockchain.UtilityToken;

        public uint BlockHeight => Blockchain.Default.Height;

        public event EventHandler<Block> PersistCompleted
        {
            add => Blockchain.PersistCompleted += value;
            remove => Blockchain.PersistCompleted -= value;
        }

        public void Initialize()
        {
            this.InitializeLocalNode();
        }

        public BlockchainStatus GetStatus()
        {
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

            var nodeCount = (uint)this.localNode.RemoteNodeCount;

            return new BlockchainStatus(Blockchain.Default.Height, Blockchain.Default.HeaderHeight,
                nextBlockProgressIsIndeterminate, nextBlockProgressFraction, timeSinceLastBlock, nodeCount);
        }

        public void Relay(Transaction transaction)
        {
            this.localNode.Relay(transaction);
        }

        public void Relay(IInventory inventory)
        {
            this.localNode.Relay(inventory);
        }

        public Transaction GetTransaction(UInt256 hash)
        {
            return Blockchain.Default.GetTransaction(hash);
        }

        public Transaction GetTransaction(UInt256 hash, out int height)
        {
            return Blockchain.Default.GetTransaction(hash, out height);
        }

        public AccountState GetAccountState(UInt160 scriptHash)
        {
            return Blockchain.Default.GetAccountState(scriptHash);
        }

        public ContractState GetContractState(UInt160 scriptHash)
        {
            return Blockchain.Default.GetContract(scriptHash);
        }

        public AssetState GetAssetState(UInt256 assetId)
        {
            return Blockchain.Default.GetAssetState(assetId);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            return Blockchain.CalculateBonus(inputs, ignoreClaimed);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            return Blockchain.CalculateBonus(inputs, heightEnd);
        }

        #endregion

        #region Private Methods 

        private void InitializeLocalNode()
        {
            // Initialize blockchain
            this.blockChain = Blockchain.RegisterBlockchain(new LevelDBBlockchain(settingsManager.LocalNodeBlockchainDataDirectoryPath));

            // Initialize local node
            TryLoadPeerState();
            this.StartLocalNode();
        }

        private void StartLocalNode()
        {
            this.localNode = new LocalNode
            {
                UpnpEnabled = true
            };

            Task.Run(() =>
            {
                ImportBlocksIfRequired();

                Blockchain.PersistCompleted += this.BlockchainPersistCompleted;

                // Start node
                this.localNode?.Start(this.settingsManager.LocalNodePort, this.settingsManager.LocalWSPort);
            });
        }

        private void BlockchainPersistCompleted(object sender, Block block)
        {
            this.timeOfLastBlock = DateTime.UtcNow;
            this.messagePublisher.Publish(new BlockchainPersistCompletedMessage());
        }

        private static void ImportBlocksIfRequired()
        {
            const string accPath = "chain.acc";
            const string accZipPath = accPath + ".zip";

            // Check if blocks need importing
            if (File.Exists(accPath))
            {
                // Import blocks
                bool importCompleted;
                using (var fileStream = new FileStream(accPath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    importCompleted = ImportBlocks(fileStream);
                }

                if (importCompleted)
                {
                    File.Delete(accPath);
                }
            }
            else if (File.Exists(accZipPath))
            {
                // Import blocks
                bool importCompleted;
                using (var fileStream = new FileStream(accZipPath, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                using (var zipStream = zip.GetEntry(accPath).Open())
                {
                    importCompleted = ImportBlocks(zipStream);
                }

                if (importCompleted)
                {
                    File.Delete(accZipPath);
                }
            }
        }

        private static bool ImportBlocks(Stream stream)
        {
            var blockchain = (LevelDBBlockchain)Blockchain.Default;
            blockchain.VerifyBlocks = false;
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

                        // TODO Check if application is exiting. If it is not, throw exception or show error message

                        return false;
                    }
                }
            }
            blockchain.VerifyBlocks = true;

            return true;
        }

        private static void TryLoadPeerState()
        {
            if (!File.Exists(PeerStatePath)) return;

            try
            {
                using (var fileStream = new FileStream(PeerStatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    LocalNode.LoadState(fileStream);
                }
            }
            catch
            {
                // Swallow exception
                // TODO Log exception somewhere
            }
        }

        private static void SavePeerState()
        {
            try
            {
                using (var fileStream = new FileStream(PeerStatePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    LocalNode.SaveState(fileStream);
                }
            }
            catch
            {
                // Swallow exception
                // TODO Log exception somewhere
            }
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
                    SavePeerState();

                    this.localNode.Dispose();
                    this.localNode = null;

                    this.blockChain.Dispose();
                    this.blockChain = null;
                    
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