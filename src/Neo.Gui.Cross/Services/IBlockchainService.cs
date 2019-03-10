using Neo.Persistence;

namespace Neo.Gui.Cross.Services
{
    public interface IBlockchainService
    {
        uint HeaderHeight { get; }

        uint Height { get; }

        Snapshot GetSnapshot();
    }
}
