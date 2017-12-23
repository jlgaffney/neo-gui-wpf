using System;
using System.Collections.Generic;
using System.IO;

using Neo.Core;
using Neo.Network;

namespace Neo.Gui.Base.Controllers
{
    public class BaseBlockchainController : IBaseBlockchainController
    {
        #region Private fields
        private const string PeerStatePath = "peers.dat";

        private readonly int localNodePort;
        private readonly int localWSPort;

        private LocalNode localNode;

        private bool initialized;
        private bool disposed;
        #endregion

        #region Constructor
        public BaseBlockchainController(int localNodePort, int localWSPort)
        {
            this.localNodePort = localNodePort;
            this.localWSPort = localWSPort;
        }
        #endregion

        #region IBaseBlockchainController implementation

        public RegisterTransaction GoverningToken => Blockchain.GoverningToken;

        public RegisterTransaction UtilityToken => Blockchain.UtilityToken;

        public virtual void Initialize()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IBaseBlockchainController));
            }

            if (this.initialized)
            {
                throw new Exception(nameof(IBaseBlockchainController) + " has already been initialized!");
            }

            this.InitializeLocalNode();

            this.initialized = true;
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, bool ignoreClaimed = true)
        {
            return Blockchain.CalculateBonus(inputs, ignoreClaimed);
        }

        public Fixed8 CalculateBonus(IEnumerable<CoinReference> inputs, uint heightEnd)
        {
            return Blockchain.CalculateBonus(inputs, heightEnd);
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

        #region Protected methods

        protected uint GetRemoteNodeCount()
        {
            return (uint)this.localNode.RemoteNodeCount;
        }

        #endregion

        #region Private methods

        private void InitializeLocalNode()
        {
            TryLoadPeerState();

            this.localNode = new LocalNode
            {
                UpnpEnabled = true
            };

            // Start node
            this.localNode?.Start(this.localNodePort, this.localWSPort);
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

                    if (this.initialized)
                    {
                        this.localNode.Dispose();
                        this.localNode = null;
                    }

                    this.disposed = true;
                }
            }
        }

        ~BaseBlockchainController()
        {
            Dispose(false);
        }

        #endregion
    }
}
