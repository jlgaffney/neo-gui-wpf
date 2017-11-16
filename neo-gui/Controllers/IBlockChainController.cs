using Neo.Core;

namespace Neo.Controllers
{
    public interface IBlockChainController
    {
        void StartLocalNode();

        void Relay(Transaction transaction);
    }
}
