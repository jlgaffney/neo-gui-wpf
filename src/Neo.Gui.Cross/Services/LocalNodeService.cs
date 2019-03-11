using Neo.Gui.Cross.Exceptions;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;

namespace Neo.Gui.Cross.Services
{
    public class LocalNodeService : ILocalNodeService
    {
        private readonly NeoSystem neoSystem;
        private readonly IWalletService walletService;

        public LocalNodeService(
            NeoSystem neoSystem,
            IWalletService walletService)
        {
            this.neoSystem = neoSystem;
            this.walletService = walletService;
        }

        public void RelayTransaction(Transaction transaction)
        {
            ThrowIfWalletNotOpen();

            walletService.CurrentWallet.ApplyTransaction(transaction);

            neoSystem.LocalNode.Tell(new LocalNode.Relay { Inventory = transaction }, null);
        }

        private void ThrowIfWalletNotOpen()
        {
            if (!walletService.WalletIsOpen)
            {
                throw new WalletNotOpenException();
            }
        }
    }
}
