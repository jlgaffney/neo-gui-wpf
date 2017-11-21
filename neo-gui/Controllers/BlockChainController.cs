using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.IO;
using Neo.Network;
using Neo.Properties;
using Neo.SmartContract;
using Neo.UI;
using Neo.UI.Base.Dispatching;
using Neo.UI.Base.Messages;
using Neo.UI.Messages;
using Neo.VM;

namespace Neo.Controllers
{
    public class BlockChainController : IBlockChainController
    {
        #region Private Fields
        private readonly IApplicationContext applicationContext;
        private readonly IMessagePublisher messagePublisher;
        
        private bool disposed = false;

        private Blockchain blockChain;

        private LocalNode localNode;

        private DateTime persistenceTime = DateTime.MinValue;
        private Timer uiUpdateTimer;
        #endregion

        #region Constructor 
        public BlockChainController(
            IApplicationContext applicationContext, 
            IMessagePublisher messagePublisher)
        {
            this.applicationContext = applicationContext;
            this.messagePublisher = messagePublisher;
        }
        #endregion

        #region IBlockChainController implementation 

        public void Setup(bool setupLocalNode = true)
        {
            if (setupLocalNode)
            {
                this.SetupLocalNode();
            }
            else
            {
                this.SetupRemoteNode();
            }
        }

        public void Relay(Transaction transaction)
        {
            this.localNode.Relay(transaction);
        }

        public void Relay(IInventory inventory)
        {
            this.localNode.Relay(inventory);
        }

        #endregion

        #region Private Methods 

        private void SetupLocalNode()
        {
            if (!RootCertificate.InstallCertificate()) return;

            PeerState.TryLoad();

            // Setup blockchain
            this.blockChain = Blockchain.RegisterBlockchain(new LevelDBBlockchain(Settings.Default.DataDirectoryPath));

            this.StartLocalNode();
        }

        private void SetupRemoteNode()
        {
            // Remote node is not supported yet
            throw new NotImplementedException();
        }

        private void StartLocalNode()
        {
            this.localNode = new LocalNode
            {
                UpnpEnabled = true
            };

            Task.Run(() =>
            {
                CheckForNewerVersion();

                ImportBlocksIfRequired();

                Blockchain.PersistCompleted += this.BlockchainPersistCompleted;

                // Start node
                this.localNode.Start(Settings.Default.NodePort, Settings.Default.WsPort);
            });

            this.uiUpdateTimer = new Timer
            {
                Interval = 500,
                Enabled = true,
                AutoReset = true
            };

            this.uiUpdateTimer.Elapsed += this.UpdateWallet;
        }

        private void UpdateWallet(object sender, ElapsedEventArgs e)
        {
            var persistenceSpan = DateTime.UtcNow - this.persistenceTime;

            this.UpdateBlockProgress(persistenceSpan);
            this.messagePublisher.Publish(new UpdateWalletMessage(persistenceSpan));
        }

        private void UpdateBlockProgress(TimeSpan persistenceSpan)
        {
            var blockProgressIndeterminate = false;
            var blockProgress = 0;

            if (persistenceSpan < TimeSpan.Zero)
            {
                persistenceSpan = TimeSpan.Zero;
            }

            if (persistenceSpan > Blockchain.TimePerBlock)
            {
                blockProgressIndeterminate = true;
            }
            else
            {
                blockProgressIndeterminate = true;
                blockProgress = persistenceSpan.Seconds;
            }

            var blockHeight = $"{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
            var nodeCount = this.localNode.RemoteNodeCount;
            var blockStatus = $"{Strings.WaitingForNextBlock}:";

            var blockProgressMessage = new BlockProgressMessage(
                blockProgressIndeterminate, 
                blockProgress, 
                blockHeight, 
                nodeCount, 
                blockStatus);
            this.messagePublisher.Publish(blockProgressMessage);
        }

        private void CheckForNewerVersion()
        {
            var latestVersion = VersionHelper.LatestVersion;
            var currentVersion = VersionHelper.CurrentVersion;

            if (latestVersion == null || latestVersion <= currentVersion) return;

            this.messagePublisher.Publish(new NewVersionAvailableMessage($"{Strings.DownloadNewVersion}: {latestVersion}"));
        }

        private void ImportBlocksIfRequired()
        {
            const string acc_path = "chain.acc";
            const string acc_zip_path = acc_path + ".zip";

            // Check if blocks need importing
            if (File.Exists(acc_path))
            {
                // Import blocks
                using (var fileStream = new FileStream(acc_path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    ImportBlocks(fileStream);
                }
                File.Delete(acc_path);
            }
            else if (File.Exists(acc_zip_path))
            {
                using (var fileStream = new FileStream(acc_zip_path, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Read))
                using (var zipStream = zip.GetEntry(acc_path).Open())
                {
                    ImportBlocks(zipStream);
                }
                File.Delete(acc_zip_path);
            }
        }

        private void ImportBlocks(Stream stream)
        {
            var blockchain = (LevelDBBlockchain)Blockchain.Default;
            blockchain.VerifyBlocks = false;
            using (var reader = new BinaryReader(stream))
            {
                var count = reader.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    var array = reader.ReadBytes(reader.ReadInt32());

                    if (height <= Blockchain.Default.Height) continue;

                    var block = array.AsSerializable<Block>();
                    Blockchain.Default.AddBlock(block);
                }
            }
            blockchain.VerifyBlocks = true;
        }

        private void BlockchainPersistCompleted(object sender, Block block)
        {
            this.messagePublisher.Publish(new BlockchainPersistCompletMessage());
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
                    this.blockChain.Dispose();

                    // Save peer state
                    PeerState.Save();

                    this.disposed = true;
                }
            }
        }

        ~BlockChainController()
        {
            Dispose(false);
        }

        #endregion
    }
}