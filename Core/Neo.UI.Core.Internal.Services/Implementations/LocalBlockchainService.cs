using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Network;
using Neo.SmartContract;
using Neo.UI.Core.Data;
using Neo.UI.Core.Helpers;
using Neo.UI.Core.Internal.Services.Interfaces;
using Neo.VM;

namespace Neo.UI.Core.Internal.Services.Implementations
{
    internal class LocalBlockchainService :
        BaseBlockchainService,
        IBlockchainService
    {
        #region Private Fields
        private const string PeerStatePath = "peers.dat";
        private static readonly TimeSpan OneSecondTimeSpan = new TimeSpan(0, 0, 1);

        private readonly IBlockchainImportService blockchainImportService;

        private bool initialized;
        private bool disposed;

        private LocalNode localNode;

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

        public void Initialize(int localNodePort, int localWSPort, string blockchainDataDirectoryPath)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IBlockchainService));
            }

            if (this.initialized)
            {
                // TODO Add exception message to string resources
                throw new Exception(nameof(IBlockchainService) + " has already been initialized!");
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


                // Setup local node
                TryLoadPeerState();
                this.localNode = new LocalNode
                {
                    UpnpEnabled = true
                };

                // Start local node
                this.localNode?.Start(localNodePort, localWSPort);

                this.initialized = true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error initializing {nameof(IBlockchainService)} {ex.Message}");
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
                nextBlockProgressIsIndeterminate, nextBlockProgressFraction,
                    timeSinceLastBlock, this.localNode.RemoteNodeCount);
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

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            return Blockchain.CalculateBonus(inputs, ignoreClaimed);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            return Blockchain.CalculateBonus(inputs, heightEnd);
        }
        
        public IDictionary<UInt160, BigInteger> GetNEP5Balances(UInt160 nep5ScriptHash, IEnumerable<UInt160> accountScriptHashes, out byte decimals)
        {
            decimals = 0;

            var acccountScriptHashArray = accountScriptHashes.ToArray();

            byte[] script;
            using (var builder = new ScriptBuilder())
            {
                foreach (var address in acccountScriptHashArray)
                {
                    builder.EmitAppCall(nep5ScriptHash, "balanceOf", address);
                }

                builder.Emit(OpCode.DEPTH, OpCode.PACK);

                builder.EmitAppCall(nep5ScriptHash, "decimals");

                script = builder.ToArray();
            }

            var engine = ApplicationEngine.Run(script);
            if (engine.State.HasFlag(VMState.FAULT)) return null;

            decimals = (byte)engine.EvaluationStack.Pop().GetBigInteger();

            var balances = engine.EvaluationStack.Pop().GetArray().Reverse().Zip(acccountScriptHashArray, (i, a) => new
            {
                Account = a,
                Value = i.GetBigInteger()
            }).ToDictionary(balance => balance.Account, balance => balance.Value);

            return balances;
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
                        SavePeerState();

                        this.localNode.Dispose();
                        this.localNode = null;


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
    }
}
