using System;
using System.IO;

using Neo.Core;
using Neo.Gui.Base.Status;
using Neo.Network;

namespace Neo.Gui.Base.Controllers
{
    internal class NetworkController : INetworkController
    {
        #region Private fields
        private const string PeerStatePath = "peers.dat";

        private LocalNode localNode;

        private bool initialized;
        private bool disposed;
        #endregion

        #region INetworkController Implementation

        public void Initialize(int localNodePort, int localWSPort)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(IBaseBlockchainController));
            }

            if (this.initialized)
            {
                throw new Exception(nameof(IBaseBlockchainController) + " has already been initialized!");
            }

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

        public NetworkStatus GetStatus()
        {
            return new NetworkStatus(this.localNode.RemoteNodeCount);
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

        ~NetworkController()
        {
            Dispose(false);
        }

        #endregion

        #region Private methods

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
