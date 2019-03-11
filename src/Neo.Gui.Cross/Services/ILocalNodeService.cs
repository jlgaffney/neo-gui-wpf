using Neo.Network.P2P.Payloads;

namespace Neo.Gui.Cross.Services
{
    public interface ILocalNodeService
    {
        void RelayTransaction(Transaction transaction);
    }
}
