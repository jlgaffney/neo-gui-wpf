using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.IO;
using Neo.Network;
using Neo.Gui.Wpf.Properties;
using Neo.UI.Base.Messages;
using Neo.UI.Messages;

namespace Neo.Controllers
{
    public class LocalBlockChainController : IBlockChainController
    {
        private const string PeerStatePath = "peers.dat";

        #region Private Fields
        private readonly IMessagePublisher messagePublisher;
        
        private bool disposed = false;

        private Blockchain blockChain;

        private LocalNode localNode;

        private DateTime timeOfLastBlock = DateTime.MinValue;
       #endregion

        #region Constructor 
        public LocalBlockChainController(
            IMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }
        #endregion

        #region IBlockChainController implementation 

        public uint BlockHeight => Blockchain.Default.Height;

        public void Initialize()
        {
            this.InitializeLocalNode();
        }

        public void Relay(Transaction transaction)
        {
            this.localNode.Relay(transaction);
        }

        public void Relay(IInventory inventory)
        {
            this.localNode.Relay(inventory);
        }

        public BlockChainStatus GetStatus()
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

            var nodeCount = (uint) this.localNode.RemoteNodeCount;

            return new BlockChainStatus(Blockchain.Default.Height, Blockchain.Default.HeaderHeight,
                nextBlockProgressIsIndeterminate, nextBlockProgressFraction, timeSinceLastBlock, nodeCount);
        }

        #endregion

        #region Private Methods 

        private void InitializeLocalNode()
        {
            if (!RootCertificate.InstallRootCertificate()) return;

            // Initialize blockchain
            this.blockChain = Blockchain.RegisterBlockchain(new LevelDBBlockchain(Settings.Default.DataDirectoryPath));

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
                this.localNode?.Start(Settings.Default.NodePort, Settings.Default.WsPort);
            });
        }

        private void BlockchainPersistCompleted(object sender, Block block)
        {
            this.timeOfLastBlock = DateTime.UtcNow;
            this.messagePublisher.Publish(new BlockchainPersistCompletMessage());
        }

        private static void ImportBlocksIfRequired()
        {
            const string acc_path = "chain.acc";
            const string acc_zip_path = acc_path + ".zip";

            // Check if blocks need importing
            if (File.Exists(acc_path))
            {
                // Import blocks
                bool importCompleted;
                using (var fileStream = new FileStream(acc_path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    importCompleted = ImportBlocks(fileStream);
                }

                if (importCompleted)
                {
                    File.Delete(acc_path);
                }
            }
            else if (File.Exists(acc_zip_path))
            {
                // Import blocks
                bool importCompleted;
                using (var fileStream = new FileStream(acc_zip_path, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                using (var zipStream = zip.GetEntry(acc_path).Open())
                {
                    importCompleted = ImportBlocks(zipStream);
                }

                if (importCompleted)
                {
                    File.Delete(acc_zip_path);
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

        ~LocalBlockChainController()
        {
            Dispose(false);
        }

        #endregion
    }
}